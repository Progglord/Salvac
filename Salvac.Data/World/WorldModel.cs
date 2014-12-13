using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using OpenTK;
using System.Collections.Generic;
using System.Text;
using Salvac.Data.Types;
using System.Globalization;

namespace Salvac.Data.World
{
    public sealed class WorldModel : IDisposable
    {
        private SQLiteConnection _connection;
        private string _sectorGeometryColumn;


        public bool IsDisposed
        { get; private set; }


        internal WorldModel(SQLiteConnection connection)
        {
            _connection = connection;
            this.IsDisposed = false;

            this.CheckTables();

            // Read geometry column
            using (var command = this.CreateCommand())
            {
                command.CommandText = @"SELECT f_geometry_column FROM geometry_columns WHERE f_table_name LIKE 'sectors' LIMIT 1";
                _sectorGeometryColumn = command.ExecuteScalar() as string;

                if (string.IsNullOrEmpty(_sectorGeometryColumn))
                    throw new InvalidDatabaseException("The world model database has no geometry for sectors.");
            }
        }

        private void CheckTables()
        {
            using (var command = this.CreateCommand())
            {
                command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name IN ('geometry_columns', 'sectors')";
                long count = (long)command.ExecuteScalar();
                if (count != 2)
                    throw new InvalidDatabaseException("The world model database is missing 'geometry_columns' or 'layers' table.");
            }
        }


        public SQLiteCommand CreateCommand()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("SectorModel");
            return _connection.CreateCommand();
        }

        public SectorView CreateSectorView(string name, IEnumerable<int> baseSectors, double distance)
        {
            // Compile command
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("CREATE TEMP VIEW {0} AS ", name);
            builder.AppendFormat("SELECT sec.id AS id, sec.name AS name, sec.{0} AS geometry ", _sectorGeometryColumn);
            builder.Append("FROM sectors AS sec ");

            if (baseSectors != null)
            {
                builder.AppendFormat("JOIN sectors AS base ON base.id IN ({0}) ", String.Join(",", baseSectors));
                //builder.AppendFormat("WHERE (ST_Contains(base.{0}, sec.{0}) OR ST_Touches(base.{0}, sec.{0})) ", _sectorGeometryColumn);
                builder.AppendFormat("WHERE ST_Contains(base.{0}, sec.{0}) OR ST_Touches(base.{0}, sec.{0}) OR ST_Distance(base.{0}, sec.{0}, 1) <= {1}", 
                    _sectorGeometryColumn, distance.ToString(CultureInfo.InvariantCulture.NumberFormat));
            }

            using (var command = this.CreateCommand())
            {
                command.CommandText = builder.ToString();
                command.ExecuteNonQuery();
            }

            return new SectorView(this, name);
        }


        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_connection != null)
                        _connection.Dispose();
                }
                this.IsDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }


        public static WorldModel Open(string file)
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");
            if (!File.Exists(file)) throw new FileNotFoundException("Could not find world model database.", file);

            string connectionString = string.Format("Data Source='{0}'", Path.GetFullPath(file));

            SQLiteConnection conn = new SQLiteConnection(connectionString);
            try
            {
                conn.Open();
                conn.EnableExtensions(true);
                conn.LoadExtension(SpatiaLiteHelper.ExtensionPath);
            }
            catch (SQLiteException ex)
            {
                throw new InvalidDatabaseException("Could not load SQLite database or SpatiaLite extension.", ex);
            }

            return new WorldModel(conn);
        }
    }
}
