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

namespace Salvac.Interface.Rendering
{
    public sealed class PilotRenderer : IRenderable
    {
        public event EventHandler Updated;

        private Vector2 _position;
        private TextRenderer _textRenderer;
        private Font _labelFont;

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


        public void Load()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("PilotRenderer");
            if (this.IsLoaded) return;

            _labelFont = new Font("Microsoft Sans Serif", 10);

            UpdatePilot();
            this.Pilot.Updated += (s, e) => 
            { 
                UpdatePilot();
                if (this.Updated != null) this.Updated(this, EventArgs.Empty);
            };

            this.IsLoaded = true;
        }

        private void UpdatePilot()
        {
            double[] xy = new double[] { this.Pilot.Position.X, this.Pilot.Position.Y };
            double[] z = new double[] { 0d };
            Reproject.ReprojectPoints(xy, z, KnownCoordinateSystems.Geographic.World.WGS1984, ProfileManager.Current.Profile.Projection, 0, 1);
            _position = new Vector2((float)xy[0], (float)xy[1]);
        }

        public void Render(Viewport viewport)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("PilotRenderer");
            if (!this.IsLoaded) return;
            if (!this.IsEnabled) return;
            if (!viewport.IsVisible(_position))
                return;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            viewport.LoadView();
            GL.Translate(_position.X, _position.Y, 0d);

            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(Vector2.Zero);
            GL.End();

            _textRenderer.DrawString(this.Pilot.Callsign, _labelFont, Brushes.White, new Vector2(10f, 10f));

            GL.PopMatrix();

#if DEBUG
            DebugScreen.DrawnPilots++;
#endif
        }


        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    _labelFont.Dispose();
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
