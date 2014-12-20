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
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Salvac.Interface.Rendering
{
    public class TextRenderer : IDisposable
    {
        private Viewport _viewport;
        private Bitmap _bitmap;
        private Graphics _graphics;
        private int _glTexture;

        public bool IsDisposed
        { get; private set; }

        public bool IsLoaded
        { get; private set; }


        public TextRenderer(Viewport viewport)
        {
            if (viewport == null) throw new ArgumentNullException("viewport");
            _viewport = viewport;

            this.IsDisposed = false;
            this.IsLoaded = false;
        }


        public void Load()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("TextRenderer");
            if (this.IsLoaded) return;

            _bitmap = new Bitmap(_viewport.Width, _viewport.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            _graphics.SmoothingMode = SmoothingMode.HighQuality;
            
            _glTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _glTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _bitmap.Width, _bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            _viewport.Resized += (s, e) =>
            {
                if (!this.IsLoaded) return;

                _graphics.Dispose();
                _bitmap.Dispose();

                _bitmap = new Bitmap(_viewport.Width, _viewport.Height);
                _graphics = Graphics.FromImage(_bitmap);
            };

            this.IsLoaded = true;
        }

        public void Begin()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("TextRenderer");
            if (!this.IsLoaded) return;

            _graphics.Clear(Color.Transparent);
        }

        public void End()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("TextRenderer");
            if (!this.IsLoaded) return;

            // Update texture
            BitmapData data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, _glTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _bitmap.Width, _bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            _bitmap.UnlockBits(data);

            // Reset world/view matrix
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // Draw texture
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 0); GL.Vertex2(0, _bitmap.Height);
            GL.TexCoord2(1, 0); GL.Vertex2(_bitmap.Width, _bitmap.Height);
            GL.TexCoord2(1, 1); GL.Vertex2(_bitmap.Width, 0);
            GL.TexCoord2(0, 1); GL.Vertex2(0, 0);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
        }


        public void DrawString(string text, Font font, Brush brush, Vector2 position)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("TextRenderer");
            if (!this.IsLoaded) return;

            // Transform position into view space
            Matrix4 matrix;
            GL.GetFloat(GetPName.ModelviewMatrix, out matrix);

            Vector3 pos = new Vector3(position.X, position.Y, 0f);
            Vector3.Transform(ref pos, ref matrix, out pos);

            // Draw the string
            _graphics.DrawString(text, font, brush, new PointF(pos.X, _bitmap.Height - pos.Y));
        }


        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (_graphics != null)
                        _graphics.Dispose();
                    if (_bitmap != null)
                        _bitmap.Dispose();
                    
                    if (GraphicsContext.CurrentContext != null)
                        GL.DeleteTexture(_glTexture);
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
