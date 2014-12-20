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
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Salvac.Interface.Rendering
{
    public sealed class Viewport
    {
        public event EventHandler Resized;


        public int Width
        { get; private set; }

        public int Height
        { get; private set; }

        public Vector2 Position
        { get; private set; }

        public float Zoom
        { get; private set; }

        public Matrix4 TranslationMatrix
        { get; private set; }

        public Matrix4 ViewMatrix
        { get; private set; }

        public RectangleF BoundingBox
        { get; private set; }


        public Viewport(int width, int height)
        {
            if (width <= 0) throw new ArgumentException("width must be positive and non-zero.", "width");
            if (height <= 0) throw new ArgumentException("height must be positive and non-zero.", "height");

            this.Width = width;
            this.Height = height;
            this.Position = Vector2.Zero;
            this.Zoom = 1f;
            this.Update();
        }


        private void Update()
        {
            this.TranslationMatrix = Matrix4.CreateTranslation((float)this.Width / 2f - this.Position.X, (float)this.Height / 2 - this.Position.Y, 0f);
            this.ViewMatrix = this.TranslationMatrix;
            this.ViewMatrix *= Matrix4.CreateTranslation(-(float)this.Width / 2f, -(float)this.Height / 2f, 0f);
            this.ViewMatrix *= Matrix4.CreateScale(this.Zoom, this.Zoom, 1);
            this.ViewMatrix *= Matrix4.CreateTranslation(+(float)this.Width / 2f, +(float)this.Height / 2f, 0f);

            float widthScale = (float)this.Width / this.Zoom;
            float heightScale = (float)this.Height / this.Zoom;
            this.BoundingBox = new RectangleF(this.Position.X - widthScale / 2f, this.Position.Y - heightScale / 2f, widthScale, heightScale);
        }

        public void Resize(int width, int height)
        {
            if (width <= 0) throw new ArgumentException("width must be positive and non-zero.", "width");
            if (height <= 0) throw new ArgumentException("height must be positive and non-zero.", "height");

            this.Width = width;
            this.Height = height;
            this.Update();

            if (this.Resized != null)
                this.Resized(this, EventArgs.Empty);
        }

        public void AdjustedMove(Vector2 translation, float zoom)
        {
            if (zoom <= -1) throw new ArgumentException("zoom may not be less than or equal to -1.", "zoom");

            if (translation != Vector2.Zero)
            {
                Vector2.Multiply(ref translation, 1f / this.Zoom, out translation);
                this.Position += translation;
            }
            if (zoom != 0f)
                this.Zoom += this.Zoom * zoom;

            this.Update();
        }

        public void PlainMove(Vector2 translation, float zoom)
        {
            if (this.Zoom + zoom <= 0) throw new ArgumentException("zoom may not result in a negative zoom value.", "zoom");

            this.Position += translation;
            this.Zoom += zoom;

            this.Update();
        }

        public void ZoomToRectangle(RectangleF rectangle)
        {
            float zoomWidth = (float)this.Width / (1.1f * rectangle.Width);
            float zoomHeight = (float)this.Height / (1.1f * rectangle.Height);
            float zoom = Math.Min(zoomWidth, zoomHeight);
            Vector2 pos = new Vector2((rectangle.Left + rectangle.Right) / 2f, (rectangle.Bottom + rectangle.Top) / 2f);

            this.PlainMove(pos - this.Position, zoom - this.Zoom);
        }


        public void LoadView()
        {
            Matrix4 view = this.ViewMatrix;
            GL.LoadMatrix(ref view);
        }

        public void LoadTranslation()
        {
            Matrix4 trans = this.TranslationMatrix;
            GL.LoadMatrix(ref trans);
        }

        
        public bool IsVisible(Vector2 vertex)
        {
            return this.IsVisible(new PointF(vertex.X, vertex.Y));
        }

        public bool IsVisible(PointF vertex)
        {
            return this.BoundingBox.Contains(vertex);
        }

        public bool IsVisible(RectangleF boundingBox)
        {
            return this.BoundingBox.IntersectsWith(boundingBox);
        }

        public bool IsCluttered(RectangleF boundingBox)
        {
            return this.IsCluttered(boundingBox.Width * boundingBox.Height);
        }

        public bool IsCluttered(float area)
        {
            return area * this.Zoom * this.Zoom <= ProfileManager.Current.Profile.DeclutterRatio;
        }
    }
}
