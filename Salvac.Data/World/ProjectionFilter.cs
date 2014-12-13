using System;
using DotSpatial.Projections;
using DotSpatial.Topology;
using Salvac.Data.Types;

namespace Salvac.Data.World
{
    public class ProjectionFilter : ICoordinateFilter
    {
        public ProjectionInfo Source
        { get; set; }

        public ProjectionInfo Target
        { get; set; }


        public ProjectionFilter(ProjectionInfo source, ProjectionInfo target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            this.Source = source;
            this.Target = target;
        }


        public void Filter(Coordinate coord)
        {
            double[] xy = new double[] { coord.X, coord.Y };
            double[] z = new double[] { coord.Z };

            Reproject.ReprojectPoints(xy, z, this.Source, this.Target, 0, 1);

            coord.X = xy[0] * Distance.NauticalMilesPerMeter;
            coord.Y = xy[1] * Distance.NauticalMilesPerMeter;
            coord.Z = z[0];
        }
    }
}
