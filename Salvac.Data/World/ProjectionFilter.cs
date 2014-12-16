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
