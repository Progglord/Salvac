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
using Salvac.Data.World;
using System.Collections.Generic;

namespace Salvac.Data.Profiles
{
    public sealed class Profile : IDisposable
    {
        public bool IsDisposed
        { get; private set; }

        public SectorView SectorView
        { get; private set; }

        public SortedEnablingSet<Layer> Layers
        { get; private set; }

        public int ProjectionID
        { get; private set; }

        public double DeclutterRatio
        { get; private set; }

        public Profile(SectorView view, int projection, double declutter)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (view.IsDisposed) throw new ObjectDisposedException("view");
            
            this.SectorView = view;
            this.ProjectionID = projection;
            this.DeclutterRatio = declutter;
            this.Layers = new SortedEnablingSet<Layer>();
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
