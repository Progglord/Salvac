﻿// Salvac
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
using System.Linq;
using System.Collections.Generic;
using Salvac.Data.Types;
using System.Net.Sockets;
using Salvac.Sessions.Fsd.Messages;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using NLog;

namespace Salvac.Sessions.Fsd
{
    public sealed class FsdSession : ISession
    {
        private const int REFRESH_INTERVAL = 500;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public event EventHandler<SessionClosedEventArgs> Closed;
        public event EventHandler<EntityEventArgs> EntityAdded;
        public event EventHandler<EntityEventArgs> EntityDestroyed;

        private Client _client;
        private Task _refreshTask;
        private CancellationTokenSource _refreshCancellation;

        public IController Controller
        { get; private set; }

        private List<IEntity> _entities;
        private object _entitiesLocker = new object();
        public IEnumerable<IEntity> Entities
        { get { return _entities.AsEnumerable(); } }


        public FsdSession(Client client, IController controller)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (controller == null) throw new ArgumentNullException("controller");

            this.Controller = controller;

            _entities = new List<IEntity>();
            _refreshCancellation = new CancellationTokenSource();

            _client = client;
            _client.MessageReceived += HandleMessage;
            _client.Disconnected += async (s, e) => 
            {
                // Cancel refresh task
                _refreshCancellation.Cancel();
                await _refreshTask;

                // Raise closed event
                if (this.Closed != null) 
                    this.Closed(s, e); 
            };

            _refreshTask = Task.Factory.StartNew(() =>
                {
                    while (!_refreshCancellation.IsCancellationRequested)
                    {
                        lock (_entitiesLocker)
                        {
                            for (int i = _entities.Count - 1; i >= 0; i--)
                            {
                                IEntity entity = _entities[i];
                                if (entity is FsdEntity)
                                    (entity as FsdEntity).Refresh();
                            }
                        }

                        try { Task.Delay(REFRESH_INTERVAL, _refreshCancellation.Token).Wait(_refreshCancellation.Token); }
                        catch (OperationCanceledException) { break; }
                    }
                }, _refreshCancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        
        public FsdEntity GetEntityFromFsdName(string fsdName)
        {
            if (string.IsNullOrEmpty(fsdName)) throw new ArgumentNullException("fsdName");

            lock (_entitiesLocker)
                return _entities.Where(e => e is FsdEntity && string.Equals((e as FsdEntity).FsdName, fsdName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() as FsdEntity;
        }


        #region Message Handling

        private void HandleMessage(object sender, MessageEventArgs e)
        {
            if (e.Message is PlanePositionMessage)
                this.HandlePlanePosition(e.Message as PlanePositionMessage);
            else if (e.Message is DeleteAtcMessage || e.Message is DeletePlaneMessage)
                this.HandleDelete(e.Message);
        }

        private void HandlePlanePosition(PlanePositionMessage message)
        {
            FsdEntity entity = this.GetEntityFromFsdName(message.Source);
            if (entity == null)
            {
                // Create new one
                FsdPlane plane = new FsdPlane(message);
                plane.TimedOut += Entity_TimedOut;
                this.OnEntityAdded(plane);
            }
            else if (entity is FsdPlane)
                (entity as FsdPlane).HandlePosition(message);
            else // if this happens, FSD is fucked up. Just notice for debug reasons, then ignore
                logger.Debug("The non-plane entity '{0}' sent PlanePositionMessage: '{1}'", entity.FsdName, message.Decompose());
        }

        private void HandleDelete(Message message)
        {
            FsdEntity entity = this.GetEntityFromFsdName(message.Source);
            if (entity != null)
                this.OnEntityDestroyed(entity);

            // Ignore if we did'nt reckognize that entity yet.
        }

        #endregion

        #region Entity Events

        private void Entity_TimedOut(object sender, EventArgs e)
        {
            // We have to delete entity manually here
            // otherwise we would run into an deadlock
            // as the refresh task locks the entities
            // (therefore entities is already locked here) 
            // and OnEntityDestroyed does, too.
            // However: EntityDestroyed event is not able to
            // alter entities, otherwise we would run into a deadlock, too.
            _entities.Remove(sender as IEntity);
            if (this.EntityDestroyed != null)
                this.EntityDestroyed(this, new EntityEventArgs(sender as IEntity));
        }

        #endregion

        #region Session Events

        private void OnEntityAdded(IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            lock (_entitiesLocker)
                _entities.Add(entity);
            if (this.EntityAdded != null)
                this.EntityAdded(this, new EntityEventArgs(entity));
        }

        private void OnEntityDestroyed(IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            lock (_entitiesLocker)
                _entities.Remove(entity);
            if (this.EntityDestroyed != null)
                this.EntityDestroyed(this, new EntityEventArgs(entity));
        }

        #endregion


        public void Close()
        {
            if (_client != null)
                _client.Disconnect(SessionClosingReason.UserDisconnect);
        }

        public void Dispose()
        {
            _client = null;
            this.Controller = null;
            _entities = null;
        }
    }
}
