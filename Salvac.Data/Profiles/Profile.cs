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
