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
