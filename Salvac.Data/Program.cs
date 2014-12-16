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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using Salvac.Data.World;
using OpenTK;
using DotSpatial.Topology;
using System.Diagnostics;
using DotSpatial.Projections;

namespace Salvac.Data
{
    public static class Program
    {
        public static void Main()
        {
        }

        private static void Do(Action action)
        {
            Stopwatch watch = Stopwatch.StartNew();
            action();
            watch.Stop();
            Console.WriteLine("    -> took {0:00.000}ms | {1} ticks", watch.Elapsed.TotalMilliseconds, watch.ElapsedTicks);
        }
    }
}
