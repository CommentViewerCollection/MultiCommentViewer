using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
namespace ryu_s.BrowserCookie
{
    /// <summary>
    /// 
    /// </summary>
    internal static class SQLiteHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbPath"></param>
        /// <returns></returns>
        public static SQLiteConnection CreateConnection(string dbPath)
        {
            return new SQLiteConnection($"Data Source={dbPath}");
        }
        public static DataTable ExecuteReader(SQLiteConnection conn, string query)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");

            DataTable dt = null;
            using (var cmd = new SQLiteCommand(query, conn))
            using (var adapter = new SQLiteDataAdapter(cmd))
            {
                dt = new DataTable();
                adapter.Fill(dt);
            }
            return dt;
        }
    }
}
