using System;
using System.Drawing;

namespace Salvac.Interface.Rendering.Geometry
{
    public interface IGeometryRenderer : IDisposable
    {
        bool IsDisposed { get; }
        RectangleF BoundingBox { get; }

        void RenderBackground();
        void RenderLines();
    }
}
