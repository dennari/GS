using EventStore.Logging;
using EventStore.Serialization;
using Growthstories.Domain.Messaging;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


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

namespace Growthstories.UI.Persistence
{
    public class SQLiteUIPersistence : IUIPersistence
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SQLiteUIPersistence));
        private readonly ISQLiteConnectionFactory ConnectionFac;
        private readonly ISerialize serializer;
        private int initialized;
        private bool disposed;
        //private readonly SQL sql;

        public SQLiteUIPersistence(
            ISQLiteConnectionFactory connectionFac,
            ISerialize serializer)
        {
            if (connectionFac == null)
                throw new ArgumentNullException("connectionFac");

            if (serializer == null)
                throw new ArgumentNullException("serializer");

            this.ConnectionFac = connectionFac;
            this.serializer = serializer;
            //this.sql = new SqlDialects.SqliteDialect();
            //var blaa = SQL.

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || this.disposed)
                return;

            Logger.Debug("Shutting down UI persistence");
            this.disposed = true;
        }

        public virtual void Initialize()
        {
            if (Interlocked.Increment(ref this.initialized) > 1)
                return;

            Logger.Debug("Initializing UI persistence");

            foreach (var st in SQL.InitializeStorage.Split(';'))
            {
                if (st.Length > 3 && st != String.Empty)
                {
                    this.ExecuteCommand(Guid.Empty, (connection, statement) =>
                        statement.ExecuteWithoutExceptions(st + ";"));
                }
            }

        }

        public virtual void Purge()
        {
            Logger.Warn("Purging UI Persistence");
            foreach (var st in SQL.PurgeStorage.Split(';'))
            {
                if (st.Length > 3 && st != String.Empty)
                {
                    this.ExecuteCommand(Guid.Empty, (connection, cmd) =>
                        cmd.ExecuteNonQuery(st + ";"));
                }
            }
        }





        public void PersistAction(ActionBase a)
        {
            this.ExecuteCommand(a.EntityId, (connection, cmd) =>
            {
                cmd.AddParameter(SQL.UserId, a.EntityId);
                cmd.AddParameter(SQL.UserRevision, a.EntityVersion);
                cmd.AddParameter(SQL.PlantId, a.PlantId);
                cmd.AddParameter(SQL.Payload, this.serializer.Serialize(a));
                return cmd.ExecuteNonQuery(SQL.PersistAction);
            });
        }

        public void PersistPlant(PlantCreated a)
        {
            this.ExecuteCommand(a.EntityId, (connection, cmd) =>
            {
                cmd.AddParameter(SQL.UserId, a.UserId);
                cmd.AddParameter(SQL.PlantId, a.EntityId);
                cmd.AddParameter(SQL.Payload, this.serializer.Serialize(a));
                return cmd.ExecuteNonQuery(SQL.PersistPlant);
            });
        }

        public enum ActionIndex
        {
            UserId,
            UserRevision,
            PlantId,
            Payload
        }

        public enum PlantIndex
        {
            UserId,
            PlantId,
            Payload
        }

        public IEnumerable<ActionBase> PlantActions(Guid PlantId)
        {
            return this.ExecuteQuery(PlantId, query =>
            {
                query.AddParameter(SQL.PlantId, PlantId);
                query.AddParameter(SQL.UserId, null);

                return query.ExecuteQuery<ActionBase>(SQL.GetActions, (stmt) => Deserialize<ActionBase>(stmt, (int)ActionIndex.Payload));
            });
        }

        public IEnumerable<ActionBase> UserActions(Guid UserId)
        {
            return this.ExecuteQuery(UserId, query =>
            {
                query.AddParameter(SQL.PlantId, null);
                query.AddParameter(SQL.UserId, UserId);

                return query.ExecuteQuery<ActionBase>(SQL.GetActions, (stmt) => Deserialize<ActionBase>(stmt, (int)ActionIndex.Payload));
            });
        }

        public IEnumerable<PlantCreated> UserPlants(Guid UserId)
        {
            return this.ExecuteQuery(UserId, query =>
            {
                query.AddParameter(SQL.PlantId, null);
                query.AddParameter(SQL.UserId, UserId);

                return query.ExecuteQuery<PlantCreated>(SQL.GetPlants, (stmt) => Deserialize<PlantCreated>(stmt, (int)PlantIndex.Payload));
            });
        }

        protected T Deserialize<T>(Sqlite3Statement stmt, int index)
        {

            return serializer.Deserialize<T>(SQLite3.ColumnByteArray(stmt, index));

        }


        protected virtual IEnumerable<T> ExecuteQuery<T>(Guid streamId, Func<SQLiteCommand, IEnumerable<T>> query)
        {
            this.ThrowWhenDisposed();

            try
            {
                return query(ConnectionFac.GetConnection().CreateCommand(""));
            }
            catch (Exception e)
            {

                Logger.Debug("StorageException: {0}", e.GetType());
                throw;
            }

        }

        protected virtual T ExecuteCommand<T>(Guid streamId, Func<SQLiteConnection, SQLiteCommand, T> command)
        {
            this.ThrowWhenDisposed();

            try
            {
                Logger.Verbose("UI Persistence: executing command");
                T rowsAffected = default(T);
                var Connection = ConnectionFac.GetConnection();
                Connection.RunInTransaction(() =>
                {
                    rowsAffected = command(Connection, Connection.CreateCommand(""));
                });


                Logger.Verbose("UI Persistence: executed command, {0} rows affeted", rowsAffected);


                return rowsAffected;
            }
            catch (Exception e)
            {
                Logger.Debug("Storage threw exception {0}", e.GetType());
                //if (!RecoverableException(e))
                //    throw new StorageException(e.Message, e);

                //Logger.Info(Messages.RecoverableExceptionCompletesScope);
                //if (scope != null)
                //   scope.Complete();

                throw;
            }
        }

        private void ThrowWhenDisposed()
        {
            if (!this.disposed)
                return;

            var msg = "UI Persistence already disposed";
            Logger.Warn(msg);
            throw new ObjectDisposedException(msg);
        }


    }
}
