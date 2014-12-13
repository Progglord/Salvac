using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using DotSpatial.Topology;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Salvac.Interface.Rendering.Geometry
{
    public sealed class PolygonRenderer : IGeometryRenderer
    {
        private int _vertexBuffer;
        private int _vertexCount;
        private int _polygonIndexBuffer;
        private int _polygonIndexCount;
        private int _lineIndexBuffer;
        private int _lineIndexCount;

        public bool IsDisposed
        { get; private set; }

        public RectangleF BoundingBox
        { get; private set; }


        public PolygonRenderer(Polygon polygon, RectangleF boundingBox)
        {
            if (polygon == null)
                throw new ArgumentNullException("polygon");
            if (boundingBox.Width < 0 || boundingBox.Height < 0)
                throw new ArgumentException("boundingBox has negative width or height.", "boundingBox");

            this.BoundingBox = boundingBox;
            this.LoadPolygon(polygon);
        }

        public PolygonRenderer(Polygon polygon)
        {
            if (polygon == null) throw new ArgumentNullException("polygon");

            IEnvelope envelope = polygon.Envelope;
            this.BoundingBox = new RectangleF((float)envelope.X, (float)envelope.Y - (float)envelope.Height, (float)envelope.Width, (float)envelope.Height);
            this.LoadPolygon(polygon);
        }

        private void LoadPolygon(Polygon polygon)
        {
            IList<Vector2> vertices;
            IList<int> lineIndices;
            IList<int> indices = PolygonTriangulator.Triangulate(polygon, out vertices, out lineIndices);
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
        }


        public void RenderBackground()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("PolygonRenderer");

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _polygonIndexBuffer);
            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, IntPtr.Zero);

            GL.DrawElements(PrimitiveType.Triangles, _polygonIndexCount, DrawElementsType.UnsignedInt, 0);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void RenderLines()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("PolygonRenderer");

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _lineIndexBuffer);
            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, IntPtr.Zero);

            GL.DrawElements(PrimitiveType.Lines, _lineIndexCount, DrawElementsType.UnsignedInt, 0);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
