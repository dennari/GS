using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#if USE_CSHARP_SQLITE
using Sqlite3 = Community.CsharpSqlite.Sqlite3;
using Sqlite3DatabaseHandle = Community.CsharpSqlite.Sqlite3.sqlite3;
using Sqlite3Statement = Community.CsharpSqlite.Sqlite3.Vdbe;
#elif USE_WP8_NATIVE_SQLITE
using Sqlite3 = Sqlite.Sqlite3;
using Sqlite3DatabaseHandle = Sqlite.Database;
using Sqlite3Statement = Sqlite.Statement;
#else
using Sqlite3DatabaseHandle = System.IntPtr;
using Sqlite3Statement = System.IntPtr;
#endif

namespace SQLite
{
    public partial class SQLiteCommand : IDisposable
    {


        public Dictionary<string, object> Bindings
        {
            get
            {
                return _bindings.ToDictionary(x => x.Name, x => x.Value);
            }
        }

        public IEnumerable<T> ExecuteQuery<T>(string sql, Func<Sqlite3Statement, T> f)
        {


            this.CommandText = sql;
            if (_conn.Trace)
            {
                Debug.WriteLine("Executing Query: " + this);
            }

            var stmt = Prepare();
            try
            {
                while (SQLite3.Step(stmt) == SQLite3.Result.Row)
                {

                    yield return f(stmt);
                    //serializer.Deserialize<ActionBase>(SQLite3.ColumnByteArray(stmt, (int)ActionIndex.Payload));

                }
            }
            finally
            {
                SQLite3.Finalize(stmt);
            }
        }




        public int ExecuteWithoutExceptions(string sql)
        {
            this.CommandText = sql;
            return ExecuteWithoutExceptions();
        }

        public int ExecuteWithoutExceptions()
        {
            try
            {
                return this.ExecuteNonQuery();
            }
            catch (Exception)
            {

                return 0;
            }
        }

        public int ExecuteNonQuery(string sql)
        {
            this.CommandText = sql;
            return ExecuteNonQuery();
        }

        public void AddParameter(string name, object val)
        {
            Bind(name, val);
        }

        public void Dispose()
        {
            this._conn.Dispose();
        }

    }


}
