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

namespace Salvac.Sessions
{
    public enum SessionClosingReason
    {
        /// <summary>
        /// User intented to close the session.
        /// </summary>
        UserDisconnect,

        /// <summary>
        /// User got kicked by a server admin.
        /// In that case reason text shall be not null.
        /// </summary>
        Kick,

        /// <summary>
        /// The connection was aborted.
        /// Most probably by a server error.
        /// </summary>
        ForcedDisconnect
    }

    public sealed class SessionClosedEventArgs : EventArgs
    {
        public SessionClosingReason Reason
        { get; private set; }

        public string KickMessage
        { get; private set; }

        public SessionClosedEventArgs(SessionClosingReason reason)
        {
            this.Reason = reason;
            this.KickMessage = null;
        }

        public SessionClosedEventArgs(SessionClosingReason reason, string kickMessage)
        {
            this.Reason = reason;
            this.KickMessage = kickMessage;
        }
    }
}
