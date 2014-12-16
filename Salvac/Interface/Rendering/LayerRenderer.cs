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
using System.Data;
using System.Drawing;
using System.Data.SQLite;
using System.Diagnostics;
using System.Collections.Generic;
using DotSpatial.Topology.Utilities;
using OpenTK.Graphics.OpenGL;
using Salvac.Data.World;
using Salvac.Data.Profiles;
using Salvac.Interface.Rendering.Geometry;

namespace Salvac.Interface.Rendering
{
    public sealed class LayerRenderer : IDisposable
    {
        private SortedList<long, IGeometryRenderer> _renderers;

        public bool IsDisposed
        { get; private set; }

        public Layer Layer
        { get; private set; }


        public LayerRenderer(Layer layer, string table)
        {
            if (layer == null) throw new ArgumentNullException("layer");
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");

            this.IsDisposed = false;
            this.Layer = layer;
            this.LoadRenderers(table);
        }

        private void LoadRenderers(string table)
        {
            using (var command = WorldManager.Current.Model.CreateCommand())
            {
                command.CommandText = string.Format("SELECT id, ST_AsBinary(ST_Transform(geometry, {0})) FROM {1}", ProfileManager.Current.Profile.ProjectionID, table);
                using (var reader = command.ExecuteReader())
                {
                    _renderers = new SortedList<long, IGeometryRenderer>();
                    while (reader.Read())
                    {
                        if (!SpatiaLiteHelper.CheckRow(reader))
                        {
                            Debug.WriteLine("{0} has an invalid row: id={1}", table, reader.GetValue(0).ToString());
                            continue;
                        }

                        WkbReader wkbReader = new WkbReader();
                        _renderers.Add(reader.GetInt64(0), GeometryRenderer.Create(wkbReader.Read(reader.GetValue(1) as byte[])));
                    }
                }
            }
        }


        private IEnumerable<IGeometryRenderer> IterateVisibleRenderers(Viewport viewport)
        {
            foreach (KeyValuePair<long, IGeometryRenderer> kvp in _renderers)
            {
                if (this.Layer.Content.IsEnabled(kvp.Key) &&
                    viewport.IsVisible(kvp.Value.BoundingBox) &&
                    !viewport.IsCluttered(kvp.Value.BoundingBox))
                    yield return kvp.Value;
            }
        }

        public void RenderBackground(Viewport viewport)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("LayerRenderer");

            GL.Color4(this.Layer.Theme.FillColor);
            foreach (IGeometryRenderer renderer in this.IterateVisibleRenderers(viewport))
            {
                renderer.RenderBackground();

#if DEBUG
                DebugInfo.DrawnSectorBackgrounds++;
#endif
            }
            GL.Color4(Color.White);
        }

        public void RenderLines(Viewport viewport)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("LayerRenderers");

            GL.Color4(this.Layer.Theme.LineColor);
            GL.LineWidth(this.Layer.Theme.LineWidth);
            if (this.Layer.Theme.EnableLineStippling)
            {
                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(this.Layer.Theme.LineStipplingFactor, this.Layer.Theme.LineStipplePattern);
            }

            foreach (IGeometryRenderer renderer in this.IterateVisibleRenderers(viewport))
            {
                renderer.RenderLines();

#if DEBUG
                DebugInfo.DrawnSectorBoundaries++;
                if (DebugInfo.DrawBoundingBoxes)
                {
                    GL.Color3(Color.Green);
                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Vertex2(renderer.BoundingBox.Left, renderer.BoundingBox.Bottom);
                    GL.Vertex2(renderer.BoundingBox.Right, renderer.BoundingBox.Bottom);
                    GL.Vertex2(renderer.BoundingBox.Right, renderer.BoundingBox.Top);
                    GL.Vertex2(renderer.BoundingBox.Left, renderer.BoundingBox.Top);
                    GL.Vertex2(renderer.BoundingBox.Left, renderer.BoundingBox.Bottom);
                    GL.End();
                    GL.Color4(this.Layer.Theme.LineColor);
                }
#endif
            }

            GL.Color4(Color.White);
            GL.LineWidth(1f);
            GL.Disable(EnableCap.LineStipple);
        }


        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (_renderers != null)
                    {
                        foreach (IGeometryRenderer renderer in _renderers.Values)
                            renderer.Dispose();
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
