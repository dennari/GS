
using System;
namespace SQLite
{
    public interface ISQLiteConnectionFactory
    {
        SQLiteConnection GetConnection();
    }

    public class DelegateConnectionFactory : ISQLiteConnectionFactory
    {
        private Func<SQLiteConnection> F;
        public DelegateConnectionFactory(Func<SQLiteConnection> f)
        {
            this.F = f;
        }

        public SQLiteConnection GetConnection()
        {
            return F();
        }
    }

}
