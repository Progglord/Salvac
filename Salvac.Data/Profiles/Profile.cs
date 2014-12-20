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
using Salvac.Data.World;
using System.Collections.Generic;
using DotSpatial.Projections;

namespace Salvac.Data.Profiles
{
    public sealed class Profile : IDisposable
    {
        public bool IsDisposed
        { get; private set; }

        public SectorView SectorView
        { get; private set; }

        public SortedEnablingSet<Sector> Sectors
        { get; private set; }

        public IList<Layer> Layers
        { get; private set; }

        public int ProjectionId
        { get; private set; }

        public ProjectionInfo Projection
        { get; private set; }

        public double DeclutterRatio
        { get; private set; }

        public Profile(SectorView view, int projectionId, ProjectionInfo projection, double declutter)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (view.IsDisposed) throw new ObjectDisposedException("view");
            
            this.SectorView = view;
            this.ProjectionId = projectionId;
            this.Projection = projection;
            this.DeclutterRatio = declutter;
            this.Sectors = new SortedEnablingSet<Sector>((s1, s2) => s1.Id.CompareTo(s2.Id));
            this.Layers = new List<Layer>();
        }


        public bool IsLayerEnabled(Layer layer)
        {
            return this.Sectors.EnabledContent.Any(s => layer.Content.Contains(s.Id));
        }

        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (this.SectorView != null && !this.SectorView.IsDisposed)
                        this.SectorView.Dispose();
                    this.SectorView = null;
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
