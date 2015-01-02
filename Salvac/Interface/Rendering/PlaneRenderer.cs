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
using Salvac.Sessions;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using DotSpatial.Projections;
using System.Threading.Tasks;
using Salvac.Data.Profiles;
using Salvac.Data.Types;

namespace Salvac.Interface.Rendering
{
    public sealed class PlaneRenderer : IRenderable
    {
        public event EventHandler Updated;

        private TextRenderer _textRenderer;
        private Viewport _viewport;

        private PlanePosition? _lastPosition;
        private Vector2 _position;
        private Vector2 _speed;

        public bool IsDisposed
        { get; private set; }

        public bool IsLoaded
        { get; private set; }

        public bool IsEnabled
        { get; set; }

        public IPlane Plane
        { get; private set; }

        public int RenderPriority
        { get { return Priorities.Pilot; } }


        public PlaneRenderer(IPlane pilot, TextRenderer textRenderer)
        {
            if (pilot == null) throw new ArgumentNullException("pilot");
            if (textRenderer == null) throw new ArgumentNullException("textRenderer");

            IsDisposed = false;
            IsLoaded = false;
            IsEnabled = true;
            Plane = pilot;

            _textRenderer = textRenderer;
            _lastPosition = null;
            _position = Vector2.Zero;
            _speed = Vector2.Zero;
        }


        public async Task LoadAsync()
        {
            if (IsDisposed) throw new ObjectDisposedException("PilotRenderer");
            if (IsLoaded) return;

            await Task.Run(() =>
            {
                IsLoaded = true;
                Plane_Update(null, EventArgs.Empty);
                Plane.Updated += Plane_Update;
            });
        }


        private void Plane_Update(object sender, EventArgs e)
        {
            if (IsDisposed || !IsLoaded) return;

            if (!_lastPosition.HasValue)
            {
                _position = Project(Plane.Position.Position)[0];
                _speed = (float)Plane.Position.GroundSpeed.AsMetersPerSecond * 60f *
                    new Vector2((float)Angle.Sin(Plane.Position.TrueHeading), (float)Angle.Cos(Plane.Position.TrueHeading));
            }
            else
            {
                Vector2[] vectors = Project(Plane.Position.Position, _lastPosition.Value.Position);
                _position = vectors[0];

                Vector2 diff = vectors[0] - vectors[1];
                if (diff.Length == 0)
                    _speed = (float)Plane.Position.GroundSpeed.AsMetersPerSecond * 60f *
                        new Vector2((float)Angle.Sin(Plane.Position.TrueHeading), (float)Angle.Cos(Plane.Position.TrueHeading));
                else
                    _speed = (float)(Plane.Position.GroundSpeed.AsMetersPerSecond * 60d) * (vectors[0] - vectors[1]).Normalized();
            }

            if (_viewport == null || IsVisible(_viewport))
            {
                if (this.Updated != null)
                    this.Updated(this, EventArgs.Empty);
            }
        }

        private Vector2[] Project(params Vector2d[] coordinates)
        {
            double[] xy = new double[coordinates.Length * 2];
            for (int i = 0; i < coordinates.Length; i++)
            {
                xy[2 * i] = coordinates[i].X;
                xy[2 * i + 1] = coordinates[i].Y;
            }

            double[] z = new double[coordinates.Length];
            Reproject.ReprojectPoints(xy, z, KnownCoordinateSystems.Geographic.World.WGS1984, ProfileManager.Current.Profile.Projection, 0, coordinates.Length);

            Vector2[] vectors = new Vector2[coordinates.Length];
            for (int i = 0; i < coordinates.Length; i++)
            {
                vectors[i] = new Vector2((float)(xy[2 * i] * ProfileManager.Current.Profile.Projection.Unit.Meters),
                    (float)(xy[2 * i + 1] * ProfileManager.Current.Profile.Projection.Unit.Meters));
            }

            return vectors;
        }

        private LabelTheme GetTheme()
        {
            if (this.Plane.IsInactive)
                return ProfileManager.Current.Profile.Theme.InactiveLabelTheme;
            else
                return ProfileManager.Current.Profile.Theme.NormalLabelTheme;
        }


        public void Render(Viewport viewport)
        {
            if (IsDisposed) throw new ObjectDisposedException("PilotRenderer");
            if (!IsLoaded) return;
            _viewport = viewport;

            if (!IsEnabled || !IsVisible(viewport)) return;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            viewport.LoadView();
            GL.Translate(_position.X, _position.Y, 0d);

            RenderSpeedVector(viewport);
            RenderPlane(viewport);
            RenderLabel(viewport);
            
            GL.PopMatrix();

#if DEBUG
            DebugScreen.DrawnPilots++;
#endif
        }

        private void RenderSpeedVector(Viewport viewport)
        {
            if (!GetTheme().EnableSpeedVector) return;

            GL.Color4(GetTheme().SpeedVectorColor);
            GL.LineWidth(GetTheme().DotLineWidth);

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Vertex2(Vector2.Zero);
                GL.Vertex2(_speed);
            }
            GL.End();

            GL.LineWidth(1f);
            GL.Color4(Color.White);
        }

        private void RenderPlane(Viewport viewport)
        {
            GL.PushMatrix();
            float scale = 1f / viewport.Zoom;
            GL.Scale(scale, scale, 1f);

            GL.Color4(GetTheme().DotLineColor);
            GL.LineWidth(GetTheme().DotLineWidth);

            GL.Begin(PrimitiveType.Lines);
            {
                RenderDotLines(GetTheme().DotType, (float)GetTheme().DotWidth);
            }
            GL.End();

            GL.LineWidth(1f);
            GL.Color4(Color.White);

            GL.PopMatrix();
        }

        private void RenderDotLines(AircraftDotType type, float width)
        {
            switch (type)
            {
                case AircraftDotType.Cross:
                    GL.Vertex2(-width, width); GL.Vertex2(width, -width);
                    GL.Vertex2(width, width); GL.Vertex2(-width, -width);
                    break;

                case AircraftDotType.Diamond:
                    GL.Vertex2(0, width); GL.Vertex2(width, 0);
                    GL.Vertex2(width, 0); GL.Vertex2(0, -width);
                    GL.Vertex2(0, -width); GL.Vertex2(-width, 0);
                    GL.Vertex2(-width, 0); GL.Vertex2(0, width);
                    break;

                case AircraftDotType.Square:
                    GL.Vertex2(-width, width); GL.Vertex2(width, width);
                    GL.Vertex2(width, width); GL.Vertex2(width, -width);
                    GL.Vertex2(width, -width); GL.Vertex2(-width, -width);
                    GL.Vertex2(-width, -width); GL.Vertex2(-width, width);
                    break;
            }
        }

        private void RenderLabel(Viewport viewport)
        {
            string label = string.Format("{0}\r\n{1:0}ft {2:0}kt", Plane.Callsign, Plane.Position.Elevation.AsFeet, Plane.Position.GroundSpeed.AsKnots);

            Brush brush = new SolidBrush(GetTheme().LabelTextColor);
            _textRenderer.DrawString(label, GetTheme().LabelTextFont, brush, new Vector2(10f, 10f));
        }


        private bool IsVisible(Viewport viewport)
        {
            return viewport.IsVisible(_position);
        }


        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                }
                IsDisposed = true;
                IsLoaded = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
