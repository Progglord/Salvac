using System;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace Salvac.Interface.Rendering
{
    public class TextRenderer : IDisposable
    {
        private MainWindow _window;
        private Bitmap _bitmap;
        private Graphics _graphics;
        private int _glTexture;

        private bool _loaded;
        private bool _disposed;

        public TextRenderer(MainWindow window)
        {
            if (window == null) throw new ArgumentNullException("window");
            _window = window;

            _loaded = false;
            _disposed = false;

            window.glWindow.Resize += (s, e) =>
                {
                    if (!_loaded) return;

                    _graphics.Dispose();
                    _bitmap.Dispose();

                    _bitmap = new Bitmap(window.glWindow.Width, window.glWindow.Height);
                    _graphics = Graphics.FromImage(_bitmap);
                };
        }


        public void Load()
        {
            if (_disposed) throw new ObjectDisposedException("TextRenderer");
            if (_loaded) return;

            _bitmap = new Bitmap(_window.glWindow.Width, _window.glWindow.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            _graphics.SmoothingMode = SmoothingMode.HighQuality;
            
            _glTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _glTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _bitmap.Width, _bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            _loaded = true;
        }

        public void Begin()
        {
            if (_disposed) throw new ObjectDisposedException("TextRenderer");
            if (!_loaded) return;

            _graphics.Clear(Color.Transparent);
        }

        public void End()
        {
            if (_disposed) throw new ObjectDisposedException("TextRenderer");
            if (!_loaded) return;

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
            if (_disposed) throw new ObjectDisposedException("TextRenderer");
            if (!_loaded) return;

            _graphics.DrawString(text, font, brush, new PointF(position.X, _bitmap.Height - position.Y));
        }


        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_graphics != null)
                        _graphics.Dispose();
                    _graphics = null;

                    if (_bitmap != null)
                        _bitmap.Dispose();
                    _bitmap = null;
                    
                    if (GraphicsContext.CurrentContext != null)
                        GL.DeleteTexture(_glTexture);
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
