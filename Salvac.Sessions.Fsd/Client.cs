// Salvac
// Copyright (C) 2014 Oliver Schmidt
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using Salvac.Sessions.Fsd.Messages;
using System.Text;
using System.Diagnostics;
using Salvac.Interface.Rendering;
using NLog;

namespace Salvac.Sessions.Fsd
{
    public sealed class Client : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private const int RECEIVE_TIMEOUT = 10;
        private const int MAX_MESSAGES_PER_LOOP = 10;
        private const int READ_BUFFER_SIZE = 256;

        public event EventHandler<SessionClosedEventArgs> Disconnected;
        public event EventHandler<MessageEventArgs> MessageReceived;

        private TcpClient _client;
        private NetworkStream _stream;
        private MessageParser _parser;
        private CancellationTokenSource _cancellationToken;
        private SessionClosedEventArgs _closingEventArgs;

        private Task _clientTask;
        private byte[] _readBuffer;
        private string _messageBuffer;
        private Queue<Message> _sendingQueue;
        private object _sendingQueueLocker = new object();

        private Task _handlingTask;
        private Queue<string> _handlingQueue;
        private object _handlingQueueLocker = new object();

        public bool IsConnected
        { get; private set; }

        public bool IsDisposed
        { get; private set; }


        public Client()
        {
            _parser = new MessageParser();
            _cancellationToken = new CancellationTokenSource();
            _sendingQueue = new Queue<Message>();
            _handlingQueue = new Queue<string>();

            this.IsConnected = false;
            this.IsDisposed = false;
        }

        public async Task ConnectAsync(string hostName, int port)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("Client");
            if (this.IsConnected) throw new InvalidOperationException("Client is already connected.");

            _client = new TcpClient();
            _client.ReceiveTimeout = RECEIVE_TIMEOUT;
            await _client.ConnectAsync(hostName, port);

            _stream = _client.GetStream();
            _clientTask = Task.Factory.StartNew(() => ClientLoop(), _cancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _handlingTask = Task.Factory.StartNew(() => RunMessageHandling(), _cancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            this.IsConnected = true;
        }

        private void ClientLoop()
        {
            _readBuffer = new byte[READ_BUFFER_SIZE];
            _messageBuffer = string.Empty;

            while (!_cancellationToken.IsCancellationRequested)
            {
                Exception ex = this.RunMessageQueue();
                if (ex != null)
                {
                    logger.Error("Client loop cancelled with exception.", ex);
                    break;
                }

#if DEBUG
                Stopwatch watch = Stopwatch.StartNew();
#endif

                ex = this.RunMessageReading();
                if (ex != null)
                {
                    logger.Error("Client loop cancelled with exception.", ex);
                    break;
                }
#if DEBUG
                DebugScreen.ClientReadingTime.AddValue(watch.Elapsed.TotalMilliseconds);
#endif
            }

            // Close connection and so on
            this.IsConnected = false;
            if (!_cancellationToken.IsCancellationRequested)
            {
                // This was a forced disconnect
                _closingEventArgs = new SessionClosedEventArgs(SessionClosingReason.ForcedDisconnect);

                _cancellationToken.Cancel();
            }

            _handlingTask.Wait(); // Wait for the message handling task to finish
            // NO event handlers are ought to wait for client task thread
            // even though they might want to cancel it
            // they shall Disconnect() instead.

            if (_client.Connected) 
                _client.Close();

            if (this.Disconnected != null)
                this.Disconnected(this, _closingEventArgs);
            this.Dispose();
        }

        private Exception RunMessageQueue()
        {
            for (int i = 0; i < MAX_MESSAGES_PER_LOOP && _sendingQueue.Count > 0 && !_cancellationToken.IsCancellationRequested; i++)
            {
                Message message;
                lock (_sendingQueueLocker)
                    message = _sendingQueue.Dequeue();

                byte[] buffer = Encoding.ASCII.GetBytes(message.Decompose());
                if (_client.Connected)
                {
                    try { _stream.Write(buffer, 0, buffer.Length); }
                    catch (IOException ex) { return ex; }
                    catch (ObjectDisposedException ex) { return ex; }
                }
                else return new ObjectDisposedException("Client is not connected anymore.");
            }

            return null;
        }

        private Exception RunMessageReading()
        {
            // Read data -> this is intended to be blocking, but just for RECEIVE_TIMEOUT time.
            int read = 0;
            if (_client.Connected)
            {
                try { read = _stream.Read(_readBuffer, 0, _readBuffer.Length); }
                catch (IOException ex) 
                {
                    if (ex.InnerException != null && ex.InnerException is SocketException)
                    {  
                        SocketException sex = ex.InnerException as SocketException;
                        if (sex.ErrorCode == 10060)
                            return null; // Connection timed out -> this can be because of too low RECEIVE_TIMEOUT. We can ignore this.
                        else
                            return sex; // Now, return socket exception. We don't need that IOException.
                    }
                    else
                        return ex;
                }
                catch (ObjectDisposedException ex) { return ex; }
            }
            else return new ObjectDisposedException("Client is not connected anymore.");
            if (read <= 0) return new ObjectDisposedException("Client is not connected anymore.");

            _messageBuffer += Encoding.ASCII.GetString(_readBuffer, 0, read);

            // Read message buffer
            int endIndex = _messageBuffer.IndexOf(Message.END);
            while (endIndex >= 0 && !_cancellationToken.IsCancellationRequested)
            {
                string message = _messageBuffer.Substring(0, endIndex);
                _messageBuffer = _messageBuffer.Remove(0, endIndex + Message.END.Length);
                endIndex = _messageBuffer.IndexOf(Message.END);

                // Add message to the handling queue and fire up handling task (if it is not enabled yet)
                lock (_handlingQueueLocker)
                    _handlingQueue.Enqueue(message);
            }

            return null;
        }

        private void RunMessageHandling()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                while (_handlingQueue.Count > 0 && !_cancellationToken.IsCancellationRequested)
                {
                    string str;
                    lock (_handlingQueueLocker)
                        str = _handlingQueue.Dequeue();

#if DEBUG
                    DebugScreen.ClientMessageHandlingLength = _handlingQueue.Count;
#endif

                    // Parse message
                    Message message;
                    try { message = _parser.Parse(str); }
                    catch (InvalidMessageException ex)
                    {
                        logger.Trace("Unable to parse message '" + str + "'.", ex);
                        continue;
                    }

                    // Handle message
                    try { this.HandleMessage(message); }
                    catch (Exception ex)
                    {
                        logger.Warn("Exception while handling message '" + str.Trim() + "'.", ex);
                        continue;
                    }
                }

                Thread.Sleep(10);
            }
        }

        private void HandleMessage(Message message)
        {
            // Raise event for now
            if (this.MessageReceived != null)
                this.MessageReceived(this, new MessageEventArgs(message));
        }


        public void Disconnect(SessionClosingReason reason, string kickMessage)
        {
            // This method just cancels the client task.
            // The client task then destroys this client
            // and afterwards raises Disconnected event.
            // DO NOT wait for the client task.

            _closingEventArgs = new SessionClosedEventArgs(reason, kickMessage);
            _cancellationToken.Cancel();
        }

        public void Disconnect(SessionClosingReason reason)
        {
            this.Disconnect(reason, null);
        }

        public void Dispose()
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;

            if (_sendingQueue != null)
                _sendingQueue.Clear();
            _sendingQueue = null;

            if (_handlingQueue != null)
                _handlingQueue.Clear();
            _handlingQueue = null;

            _readBuffer = null;
        }
    }
}
