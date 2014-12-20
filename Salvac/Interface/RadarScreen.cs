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
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using DotSpatial.Projections;
using Salvac.Data;
using Salvac.Interface.Rendering;

using Matrix4 = OpenTK.Matrix4;
using TextRenderer = Salvac.Interface.Rendering.TextRenderer;
using Salvac.Data.Types;
using Salvac.Data.Profiles;
using System.Diagnostics;
using Salvac.Sessions;
using Salvac.Data.World;

namespace Salvac.Interface
{
    public class RadarScreen : IDisposable
    {
        private GLControl _window;

        private Viewport _viewport;
        private TextRenderer _textRenderer;

        private PriorityCollection<IInputListener> _inputListeners;
        private PriorityCollection<IRenderable> _renderables;

        public bool IsDisposed
        { get; private set; }

        public bool IsLoaded
        { get; private set; }


        public RadarScreen(GLControl window)
        {
            if (window == null) throw new ArgumentNullException("window");
            _window = window;

            this.IsDisposed = false;
            this.IsLoaded = false;

            _viewport = new Viewport(window.Width, window.Height);
            _inputListeners = new PriorityCollection<IInputListener>(l => l.InputPriority);
            _renderables = new PriorityCollection<IRenderable>(r => r.RenderPriority);

            _textRenderer = new TextRenderer(_viewport);
        }

        public void AddRenderables(bool load, params IRenderable[] renderables)
        {
            if (renderables == null) throw new ArgumentNullException("renderables");
            if (renderables.Any(r => r == null || r.IsDisposed)) throw new ArgumentNullException("There is an IRenderable that is disposed or null.");

            if (load)
            {
                foreach (IRenderable renderable in renderables)
                {
                    renderable.Load();
                    renderable.Updated += (s, e) => { _window.Invalidate(); };
                }
            }
            else
            {
                foreach (IRenderable renderable in renderables)
                    renderable.Updated += (s, e) => { _window.Invalidate(); };
            }

            _renderables.AddRange(renderables);
            _inputListeners.AddRange(renderables.Where(r => r is IInputListener).Select(r => r as IInputListener));

            _window.Invalidate();
        }

        public void AddRenderables(params IRenderable[] renderables)
        {
            this.AddRenderables(false, renderables);
        }

        public void RemoveRenderables(params IRenderable[] renderables)
        {
            if (renderables == null) throw new ArgumentNullException("renderables");

            foreach (IRenderable renderable in renderables)
            {
                if (renderable != null)
                {
                    _renderables.Remove(renderable);

                    if (renderable is IInputListener)
                        _inputListeners.Remove(renderable as IInputListener);
                }
            }

            _window.Invalidate();
        }


        public void Load()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("RadarScreen");
            if (this.IsLoaded) return;
            if (!ProfileManager.Current.IsLoaded) throw new NoProfileException();

            // Load TextRenderer and Renderables
            _textRenderer.Load();
            this.AddRenderables(true, new EnvironmentRenderer(), new DebugScreen(_textRenderer));

            // Setup viewport
            LoadViewport();

            // Setup events
            _window.Resize += (s, e) =>
            { _viewport.Resize(_window.ClientSize.Width, _window.ClientSize.Height); };

            SessionManager.Current.SessionOpened += SessionLoaded;
            SessionManager.Current.SessionClosed += SessionClosed;

            // Load session if there is any
            if (SessionManager.Current.IsLoaded)
                SessionLoaded(null, EventArgs.Empty);

            this.IsLoaded = true;
        }

        private void LoadViewport()
        {
            // Get maximum bounding rectangle
            using (var cmd = WorldManager.Current.Model.CreateCommand())
            {
                string geom = string.Format("ST_Transform(geometry, {0})", ProfileManager.Current.Profile.ProjectionId);
                cmd.CommandText = string.Format("SELECT min(ST_MinX({0})) AS minX, min(ST_MinY({0})) as minY, max(ST_MaxX({0})) as maxX, max(ST_MaxY({0})) as maxY ", geom);
                cmd.CommandText += string.Format("FROM {0}", ProfileManager.Current.Profile.SectorView.Name);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read() || !SpatiaLiteHelper.CheckRow(reader))
                    {
                        Debug.WriteLine("Could not load maximum bounding rectangle for current sector view.");
                        return;
                    }

                    double minX = (double)reader["minX"];
                    double minY = (double)reader["minY"];
                    double maxX = (double)reader["maxX"];
                    double maxY = (double)reader["maxY"];

                    RectangleF boundingRectangle = new RectangleF((float)minX, (float)minY, (float)(maxX - minX), (float)(maxY - minY));
                    _viewport.ZoomToRectangle(boundingRectangle);
                }
            }
        }

        private void SessionLoaded(object sender, EventArgs e)
        {
            this.AddRenderables(true, SessionManager.Current.Session.Entities.Where(p => p is IPilot).Select(p => new PilotRenderer(p as IPilot, _textRenderer)).ToArray());

            SessionManager.Current.Session.EntityAdded += (_s, _e) =>
            {
                if (_e.Entity is IPilot)
                    this.AddRenderables(true, new PilotRenderer(_e.Entity as IPilot, _textRenderer));
            };
        }

        private void SessionClosed(object sender, EventArgs e)
        {
            this.RemoveRenderables(_renderables.Where(r => r is PilotRenderer).ToArray());
        }

        public void Render()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("RadarScreen");
            if (!this.IsLoaded) return;

            _textRenderer.Begin();

            foreach (IRenderable renderable in _renderables)
            {
                if (renderable.IsEnabled && renderable.IsLoaded)
                    renderable.Render(_viewport);
            }

            _textRenderer.End();
        }


        #region Input listeners

        public void MouseClick(MouseButtons button, Vector2 position)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("RadarScreen");
            if (!this.IsLoaded) return;

            foreach (IInputListener listener in _inputListeners)
            {
                if (listener.IsMouseOver(position))
                {
                    if (listener.MouseClick(button, position))
                        return;
                }
            }
        }

        public void MouseDoubleClick(MouseButtons button, Vector2 position)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("RadarScreen");
            if (!this.IsLoaded) return;

            foreach (IInputListener listener in _inputListeners)
            {
                if (listener.IsMouseOver(position))
                {
                    if (listener.MouseDoubleClick(button, position))
                        return;
                }
            }
        }

        public void MouseDrag(MouseButtons button, Vector2 position, Vector2 delta, float wheelDelta)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("RadarScreen");
            if (!this.IsLoaded) return;

            foreach (IInputListener listener in _inputListeners)
            {
                if (listener.IsMouseOver(position))
                {
                    if (listener.MouseDrag(button, position, delta, wheelDelta))
                        return;
                }
            }

            if (button == MouseButtons.Left)
            {
                delta.X = -delta.X;
                _viewport.AdjustedMove(delta, wheelDelta / 500f);
                _window.Invalidate();
            }
        }

        public void MouseMove(Vector2 position, float wheelDelta)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("RadarScreen");
            if (!this.IsLoaded) return;

            foreach (IInputListener listener in _inputListeners)
            {
                if (listener.IsMouseOver(position))
                {
                    if (listener.MouseMove(position, wheelDelta))
                        return;
                }
            }

            if (wheelDelta != 0)
            {
                _viewport.AdjustedMove(Vector2.Zero, wheelDelta / 1000f);
                _window.Invalidate();
            }
        }

        public bool KeyPress(Keys keys)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("RadarScreen");
            if (!this.IsLoaded) return false;

            foreach (IInputListener listener in _inputListeners)
            {
                if (listener.KeyPress(keys))
                    return true;
            }

            return false;
        }

        #endregion


        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (_textRenderer != null)
                        _textRenderer.Dispose();
                    foreach (IRenderable renderable in _renderables)
                        renderable.Dispose();
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
