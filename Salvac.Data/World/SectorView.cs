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
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Salvac.Data.World
{
    public sealed class SectorView : IDisposable
    {
        public bool IsDisposed
        { get; private set; }

        public WorldModel Model
        { get; private set; }

        public string Name
        { get; private set; }


        internal SectorView(WorldModel model, string name)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (model.IsDisposed) throw new ObjectDisposedException("model");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            this.IsDisposed = false;
            this.Model = model;
            this.Name = name;
        }


        public IEnumerable<string> GetAllNames()
        {
            using (var command = this.Model.CreateCommand())
            {
                command.CommandText = string.Format("SELECT name FROM {0}", this.Name);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        yield return reader["name"] as string;
                }
            }
        }


        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (!this.Model.IsDisposed)
                    {
                        SQLiteCommand command = null;
                        try
                        {
                            command = this.Model.CreateCommand();
                            command.CommandText = "DROP VIEW " + this.Name;
                            command.ExecuteNonQuery();
                        }
                        finally
                        {
                            if (command != null)
                                command.Dispose();
                        }
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
