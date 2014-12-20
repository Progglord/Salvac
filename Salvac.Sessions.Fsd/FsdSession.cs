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
using System.Linq;
using System.Collections.Generic;
using DotSpatial.Topology;
using Salvac.Data.Types;

namespace Salvac.Sessions.Fsd
{
    public sealed class FsdSession : ISession
    {
        public event EventHandler<EntityEventArgs> EntityAdded;

        public IController ThisController
        { get; private set; }

        private List<IEntity> _entities;
        public IEnumerable<IEntity> Entities
        { get { return _entities.AsEnumerable(); } }


        public FsdSession()
        {
            this.ThisController = new FsdController(this, "EDWW_W_CTR", 53, 54, 55);
            _entities = new List<IEntity>();
            _entities.Add(new FsdPilot(this, "DLH123", new Coordinate(8d, 53d), Distance.FromFeet(10000d)));
            _entities.Add(new FsdPilot(this, "BER155", new Coordinate(8.3d, 52.8d), Distance.FromFeet(10000d)));
            _entities.Add(new FsdPilot(this, "THY666", new Coordinate(8.2d, 52.95d), Distance.FromFeet(10000d)));
        }


        public void Close()
        {
            _entities.Clear();
        }

        public void Dispose()
        {
            // Nothing to dispose here.
        }
    }
}
