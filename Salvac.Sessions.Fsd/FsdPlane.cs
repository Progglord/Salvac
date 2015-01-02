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
using OpenTK;
using Salvac.Data.Types;
using Salvac.Sessions.Fsd.Messages;

namespace Salvac.Sessions.Fsd
{
    public sealed class FsdPlane : FsdEntity, IPlane
    {
        public string Callsign
        { get { return this.FsdName; } }

        public PlanePosition Position
        { get; private set; }

        
        public FsdPlane(PlanePositionMessage message) :
            base(message.Source)
        {
            HandlePosition(message); // Just to ensure, plane is really created by message.
        }


        public void HandlePosition(PlanePositionMessage message)
        {
            this.Position = message.Position;

            this.OnUpdated();
            this.WakeUp();
        }
    }
}
