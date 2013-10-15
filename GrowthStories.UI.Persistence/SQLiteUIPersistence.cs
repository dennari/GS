using EventStore.Logging;
using EventStore.Serialization;
using Growthstories.Domain.Messaging;
using Growthstories.Domain.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Growthstories.Domain;
using Growthstories.Core;
using Microsoft.CSharp.RuntimeBinder;

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

        public virtual void ReInitialize()
        {
            if (Interlocked.Increment(ref this.initialized) > 1)
                return;

            this.ExecuteCommand(Guid.Empty, (connection, statement) =>
                        statement.ExecuteWithoutExceptions("DROP TABLE IF EXISTS Plants;"));
            this.ExecuteCommand(Guid.Empty, (connection, statement) =>
                        statement.ExecuteWithoutExceptions("DROP TABLE IF EXISTS Actions;"));
            this.ExecuteCommand(Guid.Empty, (connection, statement) =>
                       statement.ExecuteWithoutExceptions("DROP TABLE IF EXISTS Users;"));
            this.ExecuteCommand(Guid.Empty, (connection, statement) =>
                       statement.ExecuteWithoutExceptions("DROP TABLE IF EXISTS Collaborators;"));
            //Logger.Debug(Messages.InitializingStorage);

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

        public void Save(IGSAggregate aggregate)
        {
            try
            {
                ((dynamic)this).Persist(((dynamic)aggregate).State);

            }
            catch (RuntimeBinderException)
            {

            }
        }

        void Persist(PlantActionState state)
        {
            this.ExecuteCommand(state.Id, (connection, cmd) =>
            {
                cmd.AddParameter(SQL.PlantActionId, state.Id);
                cmd.AddParameter(SQL.Created, state.Created.Ticks);
                cmd.AddParameter(SQL.UserId, state.UserId);
                cmd.AddParameter(SQL.PlantId, state.PlantId);
                cmd.AddParameter(SQL.Type, (int)state.Type);

                var payload = this.serializer.Serialize(state);

                //var debug = Encoding.UTF8.GetString(payload);

                cmd.AddParameter(SQL.Payload, payload);
                return cmd.ExecuteNonQuery(SQL.PersistAction);
            });
        }


        void Persist(PlantState a)
        {
            this.ExecuteCommand(a.Id, (connection, cmd) =>
            {
                cmd.AddParameter(SQL.UserId, a.UserId);
                cmd.AddParameter(SQL.PlantId, a.Id);
                cmd.AddParameter(SQL.GardenId, a.GardenId);
                cmd.AddParameter(SQL.Created, a.Created.Ticks);
                cmd.AddParameter(SQL.Payload, this.serializer.Serialize(a));
                return cmd.ExecuteNonQuery(SQL.PersistPlant);
            });
        }

        void Persist(UserState a)
        {
            this.ExecuteCommand(a.Id, (connection, cmd) =>
            {
                cmd.AddParameter(SQL.UserId, a.Id);
                cmd.AddParameter(SQL.GardenId, a.GardenId);
                cmd.AddParameter(SQL.Username, a.Username);
                cmd.AddParameter(SQL.Email, a.Email);
                cmd.AddParameter(SQL.Created, a.Created.Ticks);
                cmd.AddParameter(SQL.Payload, this.serializer.Serialize(a));
                return cmd.ExecuteNonQuery(SQL.PersistUser);
            });
        }

        public void SaveCollaborator(Guid collaboratorId, bool status)
        {
            this.ExecuteCommand(collaboratorId, (connection, cmd) =>
            {
                cmd.AddParameter(SQL.UserId, collaboratorId);
                cmd.AddParameter(SQL.Status, status ? 1 : 0);
                return cmd.ExecuteNonQuery("INSERT OR REPLACE INTO Collaborators (UserId,Status) VALUES(@UserId,@Status);");
            });
        }

        public enum ActionIndex
        {
            PlantActionId,
            Created,
            UserId,
            PlantId,
            Type,
            Payload
        }

        public enum PlantIndex
        {
            UserId,
            Created,
            GardenId,
            PlantId,
            Payload
        }

        public enum UserIndex
        {
            UserId,
            Created,
            GardenId,
            Username,
            Email,
            Payload,
            Collaborator
        }


        public IEnumerable<PlantActionState> GetActions(Guid? PlantActionId = null, Guid? PlantId = null, Guid? UserId = null)
        {

            var filters = new Dictionary<string, Guid?>(){
                    {"PlantActionId",PlantActionId},
                    {"PlantId",PlantId},
                    {"UserId",UserId}
                }.Where(x => x.Value.HasValue).ToArray();

            if (filters.Length == 0)
                throw new InvalidOperationException();
            return this.ExecuteQuery(PlantId ?? default(Guid), query =>
            {

                foreach (var x in filters)
                    query.AddParameter(@"@" + x.Key, x.Value);

                var queryText = string.Format(@"SELECT * FROM Actions WHERE ({0});",
                    string.Join(" AND ", filters.Select(x => string.Format(@"({0} = @{0})", x.Key))));

                return query.ExecuteQuery<PlantActionState>(queryText, (stmt) => Deserialize<PlantActionState>(stmt, (int)ActionIndex.Payload));
            });
        }

        public IEnumerable<PlantState> GetPlants(Guid? PlantId = null, Guid? GardenId = null, Guid? UserId = null)
        {

            var filters = new Dictionary<string, Guid?>(){
                    {"GardenId",GardenId},
                    {"PlantId",PlantId},
                    {"UserId",UserId}
                }.Where(x => x.Value.HasValue).ToArray();

            if (filters.Length == 0)
                throw new InvalidOperationException();
            return this.ExecuteQuery(PlantId ?? default(Guid), query =>
            {

                foreach (var x in filters)
                    query.AddParameter(@"@" + x.Key, x.Value);

                var queryText = string.Format(@"SELECT * FROM Plants WHERE ({0});",
                    string.Join(" AND ", filters.Select(x => string.Format(@"({0} = @{0})", x.Key))));

                return query.ExecuteQuery<PlantState>(queryText, (stmt) => Deserialize<PlantState>(stmt, (int)PlantIndex.Payload));
            });
        }


        public IEnumerable<UserState> GetUsers(Guid? UserId = null)
        {

            var filters = new Dictionary<string, Guid?>(){
                    {"C.UserId",UserId}
                }.Where(x => x.Value.HasValue).ToArray();

            //if (filters.Length == 0)
            //    throw new InvalidOperationException();
            return this.ExecuteQuery(UserId ?? default(Guid), query =>
            {


                var queryText = string.Format(@"SELECT U.*,C.Status FROM Users U LEFT JOIN Collaborators C ON (U.UserId = C.UserId)");
                if (filters.Length > 0)
                {
                    foreach (var x in filters)
                        query.AddParameter(@"@" + x.Key, x.Value);
                    queryText += string.Format(" WHERE ({0})",
                        string.Join(" AND ", filters.Select(x => string.Format(@"({0} = @{0})", x.Key))));
                }
                queryText += ";";



                return query.ExecuteQuery<UserState>(queryText, (stmt) =>
                {
                    var u = Deserialize<UserState>(stmt, (int)UserIndex.Payload);
                    u.IsCollaborator = SQLite3.ColumnInt(stmt, (int)UserIndex.Collaborator) > 0 ? true : false;
                    return u;
                });
            });
        }



        protected T Deserialize<T>(Sqlite3Statement stmt, int index)
        {

            var bytes = SQLite3.ColumnByteArray(stmt, index);
            // var debug = Encoding.UTF8.GetString(bytes);

            return serializer.Deserialize<T>(bytes);

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
