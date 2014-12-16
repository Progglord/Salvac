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
using System.Drawing;
using DotSpatial.Topology;

namespace Salvac.Interface.Rendering.Geometry
{
    public static class GeometryRenderer
    {
        public static IGeometryRenderer Create(IGeometry geometry)
        {
            if (geometry == null) throw new ArgumentNullException("geometry");

            if (geometry is Polygon)
                return new PolygonRenderer(geometry as Polygon);
            else
                throw new NotSupportedException("The type of geometry is not supported.");
        }

        public static IGeometryRenderer Create(IGeometry geometry, RectangleF boundingBox)
        {
            if (geometry == null) throw new ArgumentNullException("geometry");

            if (geometry is Polygon)
                return new PolygonRenderer(geometry as Polygon, boundingBox);
            else
                throw new NotSupportedException("The type of geometry is not supported.");
        }
    }
}
