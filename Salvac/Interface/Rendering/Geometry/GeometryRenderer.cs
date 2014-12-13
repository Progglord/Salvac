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
