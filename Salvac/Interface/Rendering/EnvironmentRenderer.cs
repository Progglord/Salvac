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
using System.Diagnostics;
using System.Collections.Generic;
using DotSpatial.Topology;
using DotSpatial.Topology.Utilities;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Salvac.Data;
using Salvac.Data.World;
using Salvac.Data.Profiles;
using System.Drawing;

namespace Salvac.Interface.Rendering
{
    public sealed class EnvironmentRenderer : IRenderable
    {
        public event EventHandler Updated;

        private List<KeyValuePair<long, PolygonRenderer>> _polygonRenderers;
        private Comparison<KeyValuePair<long, PolygonRenderer>> _comparison;


        public bool IsDisposed
        { get; private set; }

        public bool IsLoaded
        { get; private set; }

        public bool IsEnabled
        { get; set; }

        public int RenderPriority
        { get { return 0; } }


        public EnvironmentRenderer()
        {
            this.IsDisposed = false;
            this.IsLoaded = false;
            this.IsEnabled = true;

            _polygonRenderers = new List<KeyValuePair<long, PolygonRenderer>>();
            _comparison = (x, y) =>
                {
                    int tComp = x.Key.CompareTo(y.Key);
                    if (tComp == 0) return 0;
                    else if (x.Value.IsEnabled == y.Value.IsEnabled) return tComp;
                    else if (!x.Value.IsEnabled) return +1;
                    else return -1;
                };
        }


        public void Load()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("EnvironmentRenderer");
            if (this.IsLoaded) return;

            LoadRenderers();

            ProfileManager.Current.Profile.Sectors.EnableStatesChanged += (s, e) =>
            {
                // Update renderer enabled states
                foreach (KeyValuePair<long, PolygonRenderer> kvp in _polygonRenderers)
                    kvp.Value.IsEnabled = ProfileManager.Current.Profile.Sectors.EnabledContent.Any(x => x.Id == kvp.Key);
                _polygonRenderers.Sort(_comparison);

                // Trigger updated event
                if (Updated != null)
                    Updated(this, EventArgs.Empty);
            };

            this.IsLoaded = true;
        }

        private void LoadRenderers()
        {
            using (var cmd = WorldManager.Current.Model.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT id, ST_AsBinary(ST_Transform(geometry, {0})) AS geom FROM {1} WHERE id IN ({2})",
                    ProfileManager.Current.Profile.ProjectionId, ProfileManager.Current.Profile.SectorView.Name,
                    String.Join(",", ProfileManager.Current.Profile.Sectors.Select(s => s.Id.ToString())));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!SpatiaLiteHelper.CheckRow(reader))
                        {
                            Debug.WriteLine("Invalid row in sector view. Skipping.");
                            continue;
                        }

                        WkbReader wkb = new WkbReader();
                        IGeometry geom = wkb.Read(reader.GetStream(1));

                        if (!(geom is Polygon))
                        {
                            Debug.WriteLine("Invalid geometry type in sector view. Skipping.");
                            continue;
                        }

                        long id = reader.GetInt64(0);
                        GeometryTheme theme = ProfileManager.Current.Profile.Layers.Where(l => l.Content.Contains(id)).Select(l => l.Theme).FirstOrDefault();
                        if (theme == null)
                        {
                            Debug.WriteLine("Sector {0} has no layer assigned to it. Skipping.", id);
                            continue;
                        }

                        PolygonRenderer renderer = new PolygonRenderer(geom as Polygon, theme);
                        renderer.IsEnabled = ProfileManager.Current.Profile.Sectors.EnabledContent.Any(s => s.Id == id);
                        renderer.Load();

                        _polygonRenderers.Add(new KeyValuePair<long, PolygonRenderer>(id, renderer));
                    }
                }
            }

            _polygonRenderers.Sort(_comparison);
        }


        public void Render(Viewport viewport)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("EnvironmentRenderer");
            if (!this.IsLoaded) return;
            if (!this.IsEnabled) return;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            viewport.LoadView();

            foreach (KeyValuePair<long, PolygonRenderer> kvp in _polygonRenderers)
            {
                if (!kvp.Value.IsEnabled) break; // We can break here -> list is sorted -> enabled ones are at the top
                kvp.Value.RenderBackground(viewport);
            }

            foreach (KeyValuePair<long, PolygonRenderer> kvp in _polygonRenderers)
            {
                if (!kvp.Value.IsEnabled) break; // We can break here -> list is sorted -> enabled ones are at the top
                kvp.Value.RenderBoundaries(viewport);
            }

#if DEBUG
            if (DebugScreen.DrawBoundingBoxes)
                RenderBoundingBoxes(viewport);
#endif

            GL.PopMatrix();
        }

#if DEBUG
        private void RenderBoundingBoxes(Viewport viewport)
        {
            foreach (KeyValuePair<long, PolygonRenderer> kvp in _polygonRenderers)
            {
                if (!kvp.Value.IsEnabled) break; // We can break here -> list is sorted -> enabled ones are at the top
                if (!viewport.IsVisible(kvp.Value.BoundingBox) || viewport.IsCluttered(kvp.Value.BoundingBox)) continue;

                RectangleF box = kvp.Value.BoundingBox;

                GL.Color4(Color.Green);
                GL.Begin(PrimitiveType.LineStrip);
                {
                    GL.Vertex2(box.Left, box.Bottom);
                    GL.Vertex2(box.Right, box.Bottom);
                    GL.Vertex2(box.Right, box.Top);
                    GL.Vertex2(box.Left, box.Top);
                    GL.Vertex2(box.Left, box.Bottom);
                }
                GL.End();
                GL.Color4(Color.White);
            }
        }
#endif


        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    foreach (KeyValuePair<long, PolygonRenderer> kvp in _polygonRenderers)
                        kvp.Value.Dispose();
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
