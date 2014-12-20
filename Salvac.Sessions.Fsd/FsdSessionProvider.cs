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
using Salvac.Sessions;
using System.Windows.Forms;
using System.Threading.Tasks;

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


        public async Task<ISession> ConnectAsync()
        {
            //await Task.Delay(7000);
            return new FsdSession();
        }
    }
}
