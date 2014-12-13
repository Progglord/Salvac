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
