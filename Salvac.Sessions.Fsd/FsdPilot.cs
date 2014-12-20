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
using Salvac.Data.Types;
using System;

namespace Salvac.Sessions.Fsd
{
    public sealed class FsdPilot : IPilot
    {
        public event EventHandler Updated;

        public event EventHandler Destroyed;


        public ISession Session
        { get; private set; }

        public string Callsign
        { get; private set; }

        public Coordinate Position
        { get; private set; }

        public Distance Altitude
        { get; private set; }

        
        public FsdPilot(ISession session, string callsign, Coordinate position, Distance altitude)
        {
            this.Session = session;
            this.Callsign = callsign;
            this.Position = position;
            this.Altitude = altitude;
        }
        
    }
}
