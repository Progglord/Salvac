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
using System.Data.SQLite;

namespace Salvac.Data.Profiles
{
    public sealed class Sector
    {
        public long Id
        { get; private set; }

        public string Name
        { get; private set; }

        public Sector(long id, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            this.Id = id;
            this.Name = name;
        }

        public static Sector FromSQLiteReader(SQLiteDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException();
            
            try
            {
                return new Sector((long)reader["id"], (string)reader["name"]);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("The current row is invalid.", "reader", ex);
            }
        }
    }
}
