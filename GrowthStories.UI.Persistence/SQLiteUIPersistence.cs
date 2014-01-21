using EventStore.Serialization;
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
using System.Diagnostics;

#if USE_CSHARP_SQLITE
using Sqlite3 = Community.CsharpSqlite.Sqlite3;
using Sqlite3DatabaseHandle = Community.CsharpSqlite.Sqlite3.sqlite3;
using Sqlite3Statement = Community.CsharpSqlite.Sqlite3.Vdbe;
#elif USE_WP8_NATIVE_SQLITE
using Sqlite3Statement = Sqlite.Statement;
#else
using Sqlite3Statement = System.IntPtr;
#endif


namespace Growthstories.UI.Persistence
{
    public class SQLiteUIPersistence : IUIPersistence
    {
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

            this.GSLog().Debug("Shutting down UI persistence");
            this.disposed = true;
        }

        public virtual void Initialize()
        {
            if (Interlocked.Increment(ref this.initialized) > 1)
                return;

            this.GSLog().Debug("Initializing UI persistence");

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
            this.GSLog().Debug("Purging UI Persistence");
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
                // why is the dynamic keyword there twice ?
                this.GSLog().Info("persisting " + aggregate.State.GetType().ToString());
                ((dynamic)this).Persist(((dynamic)aggregate).State);
                this.GSLog().Info("persisted " + aggregate.State.GetType().ToString());
            }
            catch (RuntimeBinderException e)
            {
                this.GSLog().Exception(e, "RuntimeBinderException in SQLiteUIPersistence when persisting {0}", aggregate.GetType().Name);

                if (Debugger.IsAttached)
                    Debugger.Break();
            }
        }


        void Persist(IAggregateState state)
        {

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
                cmd.AddParameter(SQL.WateringScheduleId, a.WateringScheduleId);
                cmd.AddParameter(SQL.FertilizingScheduleId, a.FertilizingScheduleId);
                cmd.AddParameter(SQL.Created, a.Created.Ticks);
                cmd.AddParameter(SQL.Payload, this.serializer.Serialize(a));
                var r = cmd.ExecuteNonQuery(SQL.PersistPlant);
                //throw new Exception("Test");
                return r;
            });
        }

        void Persist(UserState a)
        {
            this.ExecuteCommand(a.Id, (connection, cmd) =>
            {
                var payload = this.serializer.Serialize(a);
                
                //this.GSLog().Info("serialized userstate: {0}", 
                //    System.Text.Encoding.UTF8.GetString(payload, 0, payload.Length));

                cmd.AddParameter(SQL.UserId, a.Id);
                cmd.AddParameter(SQL.GardenId, a.GardenId);
                cmd.AddParameter(SQL.Username, a.Username);
                cmd.AddParameter(SQL.Email, a.Email);
                cmd.AddParameter(SQL.Created, a.Created.Ticks);
                cmd.AddParameter(SQL.Payload, this.serializer.Serialize(a));
                return cmd.ExecuteNonQuery(SQL.PersistUser);
            });
        }

        void Persist(ScheduleState a)
        {
            this.ExecuteCommand(a.Id, (connection, cmd) =>
            {
                cmd.AddParameter(SQL.ScheduleId, a.Id);
                cmd.AddParameter(SQL.Payload, this.serializer.Serialize(a));
                return cmd.ExecuteNonQuery(SQL.PersistSchedule);
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
            WateringScheduleId,
            FertilizingScheduleId,
            Payload,
            WateringSchedulePayload,
            FertilizingSchedulePayload
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


        public IEnumerable<PlantActionState> GetActions(
            Guid? PlantActionId = null,
            Guid? PlantId = null,
            Guid? UserId = null,
            PlantActionType? type = null,
            int? limit = null,
            bool? isOrderAsc = null
            )
        {


            var filters = new Dictionary<string, object>(){
                    {"PlantActionId",PlantActionId},
                    {"PlantId",PlantId},
                    {"UserId",UserId},
                    {"Type",type}
                }
                .Select(x =>
                {
                    return new { Key = x.Key, Value = x.Value != null ? x.Value.ToString() : null };
                })
                .Where(x => x.Value != null)
                .ToArray();

            if (filters.Length == 0)
                throw new InvalidOperationException();
            return this.ExecuteQuery(PlantId ?? default(Guid), query =>
            {
                foreach (var x in filters)
                    query.AddParameter(@"@" + x.Key, x.Value);

                var queryText = string.Format(@"SELECT * FROM Actions WHERE ({0}) ORDER BY Created {2} LIMIT {1};",
                    string.Join(" AND ", filters.Select(x => string.Format(@"({0} = @{0})", x.Key))),
                    Math.Min(limit ?? 100, 100),
                    isOrderAsc.HasValue && !isOrderAsc.Value ? "DESC" : "");
                this.GSLog().Info("GetActions: {0}", queryText);
                return query.ExecuteQuery<PlantActionState>(queryText, (stmt) => Deserialize<PlantActionState>(stmt, (int)ActionIndex.Payload));
            });
        }

        public IEnumerable<Tuple<PlantState, ScheduleState, ScheduleState>> GetPlants(Guid? PlantId = null, Guid? GardenId = null, Guid? UserId = null)
        {

            var filters = new Dictionary<string, Guid?>(){
                    {"GardenId",GardenId},
                    {"PlantId",PlantId},
                    {"UserId",UserId}
                }.Where(x => x.Value.HasValue).ToArray();

            if (filters.Length == 0)
                throw new InvalidOperationException("GetPlants: no filters");
            return this.ExecuteQuery(PlantId ?? default(Guid), query =>
            {

                foreach (var x in filters)
                    query.AddParameter(@"@" + x.Key, x.Value);

                var queryText = string.Format(@"SELECT P.*, SW.Payload, SF.Payload FROM Plants P 
                    LEFT JOIN Schedules SW ON P.WateringScheduleId = SW.ScheduleId 
                    LEFT JOIN Schedules SF ON P.FertilizingScheduleId = SF.ScheduleId 
                    WHERE ({0}) ;", string.Join(" AND ", filters.Select(x => string.Format(@"(P.{0} = @{0})", x.Key))));
                this.GSLog().Info("GetPlants: {0}", queryText);

                return query.ExecuteQuery(queryText, (stmt) =>
                {
                    var plant = Deserialize<PlantState>(stmt, (int)PlantIndex.Payload);
                    var wateringSchedule = Deserialize<ScheduleState>(stmt, (int)PlantIndex.WateringSchedulePayload);
                    var fertilizingSchedule = Deserialize<ScheduleState>(stmt, (int)PlantIndex.FertilizingSchedulePayload);

                    return Tuple.Create(plant, wateringSchedule, fertilizingSchedule);
                });
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


                this.GSLog().Info("GetUsers: {0}", queryText);

                return query.ExecuteQuery<UserState>(queryText, (stmt) =>
                {
                    
                    
                    var u = Deserialize<UserState>(stmt, (int)UserIndex.Payload);
                    u.IsCollaborator = SQLite3.ColumnInt(stmt, (int)UserIndex.Collaborator) > 0 ? true : false;
                    
                    this.GSLog().Info("unserialized user friends is {0}", u.Friends);
                    if (u.Friends != null)
                    {
                        this.GSLog().Info("unserialized user friends count is {0}", u.Friends.Count());
                    }
                    
                    return u;
                });
            });
        }


        protected T Deserialize<T>(Sqlite3Statement stmt, int index) where T : class
        {
            var bytes = SQLite3.ColumnByteArray(stmt, index);

            // var debug = Encoding.UTF8.GetString(bytes);
            if (bytes == null || bytes.Length == 0)
                return null;

            //this.GSLog().Info("deserialized something: {0}", System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length));
            return serializer.Deserialize<T>(bytes);
        }


        protected virtual IEnumerable<T> ExecuteQuery<T>(Guid streamId, Func<SQLiteCommand, IEnumerable<T>> query)
        {
            this.ThrowWhenDisposed();
            SQLiteConnection connection = null;
            SQLiteCommand command = null;
            IEnumerable<T> results = null;

            try
            {
                connection = ConnectionFac.GetConnection();
                command = connection.CreateCommand("");
                this.GSLog().Verbose("UI Persistence: executing query");
                results = query(command);
                this.GSLog().Verbose("UI Persistence: executed query");
                return results;

            }
            catch (Exception e)
            {

                this.GSLog().Exception(e, "ExecuteQuery");
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw;
            }

        }

        protected virtual T ExecuteCommand<T>(Guid streamId, Func<SQLiteConnection, SQLiteCommand, T> executor)
        {
            this.ThrowWhenDisposed();
            SQLiteConnection connection = null;
            SQLiteCommand command = null;
            T results = default(T);

            try
            {
                connection = ConnectionFac.GetConnection();
                command = connection.CreateCommand("");
                this.GSLog().Verbose("UI Persistence: executing command");
                results = executor(connection, command);
                this.GSLog().Verbose("UI Persistence: executed command {1}, {0} rows affeted", results, command.CommandText);
                return results;
            }
            catch (Exception e)
            {
                object payload = null;
                if (command != null && command.Bindings.TryGetValue(SQL.Payload, out payload))
                {

                    string payloadString = string.Empty;
                    byte[] payloadBytes = payload as byte[];
                    if (payloadBytes != null)
                        payloadString = Encoding.UTF8.GetString(payloadBytes, 0, Math.Min(payloadBytes.Length, 2 * 1024));

                    this.GSLog().Exception(e, "ExecuteCommand\n Query: {0}\n Payload:\n{1}", command.CommandText, payloadString);

                }
                else
                    this.GSLog().Exception(e, "ExecuteCommand");

                if (Debugger.IsAttached)
                    Debugger.Break();

                throw;


            }



        }

        private void ThrowWhenDisposed()
        {
            if (!this.disposed)
                return;

            var msg = "UI Persistence already disposed";
            this.GSLog().Warn(msg);
            throw new ObjectDisposedException(msg);
        }


        IGSLog IHasLogger.Logger { get; set; }

    }
}
