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

namespace Salvac.Interface
{
    public class RadarScreen : IMouseListener, IDisposable
    {
        private MainWindow _window;
        private bool _disposed;
        private bool _loaded;

        private Viewport _viewport;

        private TextRenderer _textRenderer;
        private IList<LayerRenderer> _layerRenderers;

        private Font _labelFont;


        public RadarScreen(MainWindow window)
        {
            if (window == null) throw new ArgumentNullException("window");
            _window = window;

            _disposed = false;
            _loaded = false;

            _viewport = new Viewport(window.glWindow.Width, window.glWindow.Height);
            _viewport.PlainMove(Vector2.Zero, 0.0004f - 1f);

            _textRenderer = new TextRenderer(window);
            _layerRenderers = new List<LayerRenderer>();

            _labelFont = new Font("Terminal", 10);
        }

        public void Load()
        {
            if (_disposed) throw new ObjectDisposedException("RadarScreen");
            if (_loaded) return;

            _textRenderer.Load();
            foreach (Layer layer in ProfileManager.Current.Profile.Layers)
                _layerRenderers.Add(new LayerRenderer(layer, ProfileManager.Current.Profile.SectorView.Name));

            double[] xy = new double[] { 8d, 53d };
            double[] z = new double[1];

            // SRID 4839
            var proj = ProjectionInfo.FromProj4String("+proj=lcc +lat_1=48.66666666666666 +lat_2=53.66666666666666 +lat_0=51 +lon_0=10.5 +x_0=0 +y_0=0 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs");
            Reproject.ReprojectPoints(xy, z, KnownCoordinateSystems.Geographic.World.WGS1984, proj, 0, 1);
            Vector2 pos = new Vector2();
            pos.X = (float)Distance.FromMeters(xy[0]).AsNauticalMiles;
            pos.Y = (float)Distance.FromMeters(xy[1]).AsNauticalMiles;
            _viewport.PlainMove(pos, 0f);

            _window.AddMouseListener(this);
            _window.glWindow.Resize += (s, e) =>
            {
                _viewport.Resize(_window.glWindow.Width, _window.glWindow.Height);
            };

            _loaded = true;
        }


        public void Render()
        {
            if (_disposed) throw new ObjectDisposedException("RadarScreen");
            if (!_loaded) return;

#if DEBUG
            DebugInfo.DrawnSectorBackgrounds = 0;
            DebugInfo.DrawnSectorBoundaries = 0;
#endif

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();

            _viewport.LoadView();


            foreach (LayerRenderer renderer in _layerRenderers.Where(l => ProfileManager.Current.Profile.Layers.IsEnabled(l.Layer)))
                renderer.RenderBackground(_viewport);
            foreach (LayerRenderer renderer in _layerRenderers.Where(l => ProfileManager.Current.Profile.Layers.IsEnabled(l.Layer)))
                renderer.RenderLines(_viewport);

            _textRenderer.Begin();
#if DEBUG
            _textRenderer.DrawString(string.Format("Time: {5:00.000}ms ({6:000} FPS), Viewport: {0:000.000}, {1:000.000} Zoom: {2:00.00}, Rendered Backgrounds: {3}, Rendered Boundaries: {4}", 
                _viewport.Position.X, _viewport.Position.Y, _viewport.Zoom, DebugInfo.DrawnSectorBackgrounds, DebugInfo.DrawnSectorBoundaries,
                DebugInfo.LastFrameTime * 1000d, 1 / DebugInfo.LastFrameTime),
                _labelFont, Brushes.White, new Vector2(0, 30));
            _textRenderer.End();
#endif

            GL.PopMatrix();
        }

        private void DrawLabel(string callsign, string flightLevel, string verticalRate, string speed, string labelWp, string destination, string aircraft, Vector2 position)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(callsign);
            builder.Append(flightLevel);
            builder.Append(" ");
            builder.AppendLine(verticalRate);
            builder.Append(speed);
            builder.Append(" ");
            builder.AppendLine(labelWp);
            builder.Append(destination);
            builder.Append(" ");
            builder.Append(aircraft);

            _textRenderer.DrawString(builder.ToString(), _labelFont, Brushes.White, position);
        }

        #region User Interaction

        //public void EnableLayer(string layer, bool enabled)
        //{
        //    LayerRenderer renderer = _layerRenderers.Where(r => string.Equals(r.Layer.Name, layer)).FirstOrDefault();
        //    if (renderer != null)
        //        renderer.Enabled = enabled;
        //}

        #endregion

        #region IMouseListener

        public int Priority
        {
            get { return -1; }
        }

        public bool IsMouseOverListener(Vector2 position)
        {
            if (_disposed) throw new ObjectDisposedException("RadarScreen");
            if (!_loaded) return false;

            return _window.glWindow.ClientRectangle.Contains((int)position.X, (int)position.Y);
        }

        public void MouseClick(MouseButtons button, Vector2 position)
        {
            if (_disposed) throw new ObjectDisposedException("RadarScreen");
            if (!_loaded) return;
        }

        public void MouseDoubleClick(MouseButtons button, Vector2 position)
        {
            if (_disposed) throw new ObjectDisposedException("RadarScreen");
            if (!_loaded) return;
        }

        public void MouseDrag(MouseButtons button, Vector2 delta, float wheelDelta)
        {
            if (_disposed) throw new ObjectDisposedException("RadarScreen");
            if (!_loaded) return;

            if (button == MouseButtons.Left)
            {
                delta.X = -delta.X;
                _viewport.AdjustedMove(delta, wheelDelta / 500f);
                _window.glWindow.Invalidate();
            }
        }

        public void MouseMove(Vector2 position, float wheelDelta)
        {
            if (_disposed) throw new ObjectDisposedException("RadarScreen");
            if (!_loaded) return;

            if (wheelDelta != 0)
            {
                _viewport.AdjustedMove(Vector2.Zero, wheelDelta / 1000f);
                _window.glWindow.Invalidate();
            }
        }

        #endregion

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_textRenderer != null)
                        _textRenderer.Dispose();
                    if (_layerRenderers != null)
                    {
                        foreach (LayerRenderer renderer in _layerRenderers)
                            renderer.Dispose();
                    }
                    _labelFont.Dispose();
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
