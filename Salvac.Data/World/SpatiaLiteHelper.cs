using System;
using System.Data.SQLite;

namespace Salvac.Data.World
{
    public static class SpatiaLiteHelper
    {
        public static readonly string ExtensionPath = "mod_spatialite.dll";


        public static bool CheckRow(SQLiteDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.IsDBNull(i))
                    return false;
            }
            return true;
        }
    }
}
