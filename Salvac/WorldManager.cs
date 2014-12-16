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
