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
using DotSpatial.Topology;
using Salvac.Data.Profiles;

namespace Salvac.Interface.Rendering
{
    public sealed class PilotRenderer : IRenderable
    {
        public event EventHandler Updated;

        private TextRenderer _textRenderer;

        private Vector2 _position;
        private Vector2 _speed;
        private Viewport _viewport;

        public bool IsDisposed
        { get; private set; }

        public bool IsLoaded
        { get; private set; }

        public bool IsEnabled
        { get; set; }

        public IPilot Pilot
        { get; private set; }

        public int RenderPriority
        { get { return Priorities.Pilot; } }


        public PilotRenderer(IPilot pilot, TextRenderer textRenderer)
        {
            if (pilot == null) throw new ArgumentNullException("pilot");
            if (textRenderer == null) throw new ArgumentNullException("textRenderer");

            this.IsDisposed = false;
            this.IsLoaded = false;
            this.IsEnabled = true;
            this.Pilot = pilot;

            _textRenderer = textRenderer;
        }


        public async Task LoadAsync()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("PilotRenderer");
            if (this.IsLoaded) return;

            await Task.Run(() =>
            {
                this.IsLoaded = true;
                Pilot_Update(null, EventArgs.Empty);
                this.Pilot.Updated += Pilot_Update;
            });
        }


        private void Pilot_Update(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsLoaded) return;

            if (this.Pilot.LastPosition == null)
            {
                _position = this.Project(this.Pilot.Position)[0];
                _speed = Vector2.Zero;
            }
            else
            {
                Vector2[] vectors = this.Project(this.Pilot.Position, this.Pilot.LastPosition);
                _position = vectors[0];

                Vector2 diff = vectors[0] - vectors[1];
                if (diff.Length == 0)
                    _speed = Vector2.Zero;
                else
                    _speed = (float)(this.Pilot.GroundSpeed.AsMetersPerSecond * 60d) * (vectors[0] - vectors[1]).Normalized();
            }

            if (_viewport == null || this.IsVisible(_viewport))
            {
                if (this.Updated != null)
                    this.Updated(this, EventArgs.Empty);
            }
        }

        private Vector2[] Project(params Coordinate[] coordinates)
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
            if (this.Pilot.IsInactive)
                return ProfileManager.Current.Profile.Theme.InactiveLabelTheme;
            else
                return ProfileManager.Current.Profile.Theme.NormalLabelTheme;
        }


        public void Render(Viewport viewport)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("PilotRenderer");
            if (!this.IsLoaded) return;
            _viewport = viewport;

            if (!this.IsEnabled) return;
            if (!this.IsVisible(viewport)) return;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            viewport.LoadView();
            GL.Translate(_position.X, _position.Y, 0d);

            RenderSpeedVector(viewport);
            RenderPilot(viewport);
            RenderLabel(viewport);
            
            GL.PopMatrix();

#if DEBUG
            DebugScreen.DrawnPilots++;
#endif
        }

        private void RenderSpeedVector(Viewport viewport)
        {
            if (!this.GetTheme().EnableSpeedVector) return;

            GL.Color4(this.GetTheme().SpeedVectorColor);
            GL.LineWidth(this.GetTheme().DotLineWidth);

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Vertex2(Vector2.Zero);
                GL.Vertex2(_speed);
            }
            GL.End();

            GL.LineWidth(1f);
            GL.Color4(Color.White);
        }

        private void RenderPilot(Viewport viewport)
        {
            GL.PushMatrix();
            float scale = 1f / viewport.Zoom;
            GL.Scale(scale, scale, 1f);

            GL.Color4(this.GetTheme().DotLineColor);
            GL.LineWidth(this.GetTheme().DotLineWidth);

            GL.Begin(PrimitiveType.Lines);
            {
                RenderDotLines(this.GetTheme().DotType, (float)this.GetTheme().DotWidth);
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
            Brush brush = new SolidBrush(this.GetTheme().LabelTextColor);
            _textRenderer.DrawString(this.Pilot.Callsign, this.GetTheme().LabelTextFont, brush, new Vector2(10f, 10f));
        }


        private bool IsVisible(Viewport viewport)
        {
            return viewport.IsVisible(_position);
        }


        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
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
