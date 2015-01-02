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

using DotSpatial.Topology;
using OpenTK;
using Salvac.Data.Types;
using Salvac.Sessions.Fsd.Messages;
using System;
using System.Diagnostics;

namespace Salvac.Sessions.Fsd
{
    public sealed class FsdPilot : FsdEntity, IPilot
    {
        private bool _hadFirstPositionReport;

        public string Callsign
        { get { return this.FsdName; } }

        public Coordinate Position
        { get; set; }

        public Coordinate LastPosition
        { get; private set; }

        public Speed GroundSpeed
        { get; private set; }

        public Distance Altitude
        { get; set; }

        
        public FsdPilot(string fsdName) :
            base(fsdName)
        {
            _hadFirstPositionReport = false;

            this.Position = null;
            this.LastPosition = null;
            this.GroundSpeed = Speed.Zero;
            this.Altitude = Distance.Zero;
        }


        public void HandlePosition(PilotPositionMessage message)
        {
            if (_hadFirstPositionReport)
                this.LastPosition = this.Position;
            this.Position = message.Position;
            _hadFirstPositionReport = true;

            this.GroundSpeed = message.GroundSpeed;
            this.Altitude = message.TrueAltitude;

            this.OnUpdated();
            this.WakeUp();
        }
    }
}
