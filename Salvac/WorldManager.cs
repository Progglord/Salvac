using System;
using System.IO;
using Salvac.Data.World;
using System.Collections.Generic;

namespace Salvac
{
    public sealed class WorldManager : IDisposable
    {
        #region Singleton

        private static WorldManager _current;
        public static WorldManager Current
        {
            get
            {
                if (_current == null)
                    _current = new WorldManager();
                return _current;
            }
        }

        #endregion


        public bool IsLoaded
        { get; private set; }

        public WorldModel Model
        { get; private set; }


        private WorldManager()
        {
            this.IsLoaded = false;
            this.Model = null;
        }


        public void LoadModel(string file)
        {
            if (string.IsNullOrEmpty("file")) throw new ArgumentNullException("file");
            if (!File.Exists(file)) throw new FileNotFoundException("Could not find world model database.", file);

            if (this.Model != null && !this.Model.IsDisposed)
                this.Model.Dispose();

            this.Model = WorldModel.Open(file);
            this.IsLoaded = true;
        }


        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Model != null && !this.Model.IsDisposed)
                    this.Model.Dispose();
                this.Model = null;
            }

            this.IsLoaded = false;
            _current = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
