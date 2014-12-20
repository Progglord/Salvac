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
using System.Drawing;
using System.Collections.Generic;
using DotSpatial.Topology;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Salvac.Data.Profiles;

namespace Salvac.Interface.Rendering
{
    public sealed class PolygonRenderer
    {
        private Polygon _polygon;
        private GeometryTheme _theme;

        private int _vertexBuffer;
        private int _vertexCount;
        private int _polygonIndexBuffer;
        private int _polygonIndexCount;
        private int _lineIndexBuffer;
        private int _lineIndexCount;

        public bool IsDisposed
        { get; private set; }

        public bool IsLoaded
        { get; private set; }

        public bool IsEnabled
        { get; set; }

        public RectangleF BoundingBox
        { get; private set; }


        public PolygonRenderer(Polygon polygon, GeometryTheme theme, RectangleF boundingBox)
        {
            if (polygon == null) throw new ArgumentNullException("polygon");
            if (theme == null) throw new ArgumentNullException("theme");
            if (boundingBox.Width < 0 || boundingBox.Height < 0)
                throw new ArgumentException("boundingBox has negative width or height.", "boundingBox");

            this.IsDisposed = false;
            this.IsLoaded = false;
            this.IsEnabled = true;
            this.BoundingBox = boundingBox;

            _polygon = polygon;
            _theme = theme;
        }

        public PolygonRenderer(Polygon polygon, GeometryTheme theme)
        {
            if (polygon == null) throw new ArgumentNullException("polygon");
            if (theme == null) throw new ArgumentNullException("theme");

            this.IsDisposed = false;
            this.IsLoaded = false;
            this.IsEnabled = true;

            IEnvelope envelope = polygon.Envelope;
            this.BoundingBox = new RectangleF((float)envelope.X, (float)envelope.Y - (float)envelope.Height, (float)envelope.Width, (float)envelope.Height);

            _polygon = polygon;
            _theme = theme;
        }


        public void Load()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("PolygonRenderer");
            if (this.IsLoaded) return;

            IList<Vector2> vertices;
            IList<int> lineIndices;
            IList<int> indices = PolygonTriangulator.Triangulate(_polygon, out vertices, out lineIndices);
            _vertexCount = vertices.Count;

            // Create vertex buffer
            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vector2.SizeInBytes * _vertexCount), vertices.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Create polygon index buffer
            _polygonIndexCount = indices.Count;
            _polygonIndexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _polygonIndexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(_polygonIndexCount * sizeof(int)), indices.ToArray(), BufferUsageHint.StaticDraw);

            // Create line index buffer
            _lineIndexCount = lineIndices.Count;
            _lineIndexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _lineIndexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(_lineIndexCount * sizeof(int)), lineIndices.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            // We don't need the plain polygon anymore
            _polygon = null;

            this.IsLoaded = true;
        }


        public void RenderBackground(Viewport viewport)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("PolygonRenderer");
            if (!this.IsLoaded) return;
            if (!this.IsEnabled) return;
            if (!viewport.IsVisible(BoundingBox) || viewport.IsCluttered(BoundingBox)) return;

            GL.Color4(_theme.FillColor);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _polygonIndexBuffer);
            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, IntPtr.Zero);

            GL.DrawElements(PrimitiveType.Triangles, _polygonIndexCount, DrawElementsType.UnsignedInt, 0);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.Color4(Color.White);

#if DEBUG
            DebugScreen.DrawnEnvironment++;
#endif
        }

        public void RenderBoundaries(Viewport viewport)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("PolygonRenderer");
            if (!this.IsLoaded) return;
            if (!this.IsEnabled) return;
            if (!viewport.IsVisible(BoundingBox) || viewport.IsCluttered(BoundingBox)) return;

            GL.Color4(_theme.LineColor);
            GL.LineWidth(_theme.LineWidth);
            if (_theme.EnableLineStippling)
            {
                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(_theme.LineStipplingFactor, _theme.LineStipplePattern);
            }

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _lineIndexBuffer);
            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, IntPtr.Zero);

            GL.DrawElements(PrimitiveType.Lines, _lineIndexCount, DrawElementsType.UnsignedInt, 0);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.Disable(EnableCap.LineStipple);
            GL.LineWidth(1f);
            GL.Color4(Color.White);
        }


        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (GraphicsContext.CurrentContext != null)
                    {
                        if (_vertexBuffer != 0)
                            GL.DeleteBuffer(_vertexBuffer);
                        _vertexBuffer = 0;

                        if (_polygonIndexBuffer != 0)
                            GL.DeleteBuffer(_polygonIndexBuffer);
                        _polygonIndexBuffer = 0;

                        if (_lineIndexBuffer != 0)
                            GL.DeleteBuffer(_lineIndexBuffer);
                        _lineIndexBuffer = 0;
                    }
                }
                this.IsDisposed = true;
                this.IsLoaded = false;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
