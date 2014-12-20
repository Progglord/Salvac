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
using DotSpatial.Topology;
using Salvac.Data.Types;
using System.Collections.Generic;

namespace Salvac.Sessions
{
    public interface IController : IEntity
    {
        string Callsign { get; }

        /// <summary>
        /// A list of sectors that this controller controls.
        /// </summary>
        IList<long> Sectors { get; }
    }
}
