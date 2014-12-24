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
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading.Tasks;
using Salvac.Sessions;

namespace Salvac.Sessions.Fsd
{
    public sealed class FsdSessionProvider : ISessionProvider
    {
        private FsdConnectDialog _connectDialog;

        public string Name
        { get { return "FSD"; } }

        public Control ConnectDialog
        { get { return _connectDialog; } }


        public FsdSessionProvider()
        {
            _connectDialog = new FsdConnectDialog();
        }


        private byte[] _readBuffer;
        public async Task<ISession> ConnectAsync()
        {
            if (string.IsNullOrEmpty(_connectDialog.txtHost.Text))
                throw new InvalidOperationException("Invalid host.");
            if (string.IsNullOrEmpty(_connectDialog.txtCallsign.Text))
                throw new InvalidOperationException("Invalid callsign.");

            Client client = new Client();
            await client.ConnectAsync(_connectDialog.txtHost.Text, (int)_connectDialog.txtPort.Value);

            IController controller = new FsdController(_connectDialog.txtCallsign.Text, _connectDialog.txtCallsign.Text, 53, 54, 55);
            return new FsdSession(client, controller);
        }
    }
}
