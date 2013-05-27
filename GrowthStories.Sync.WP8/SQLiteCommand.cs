using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.Persistence.SqlPersistence;
using EventStore;
using EventStore.Serialization;
using EventStore.Persistence;

namespace SQLite
{
    public partial class SQLiteCommand : IDisposable
    {

        private const int StreamIdIndex = 0;
        private const int StreamRevisionIndex = 1;
        private const int CommitIdIndex = 2;
        private const int CommitSequenceIndex = 3;
        private const int CommitStampIndex = 4;
        private const int HeadersIndex = 5;
        private const int PayloadIndex = 6;
        private const int SnapshotPayloadIndex = 2;
        private const int HeadRevisionIndex = 1;
        private const int SnapshotRevisionIndex = 2;

        public IEnumerable<Commit> ExecuteCommitQuery(string sql, ISerialize serializer)
        {
            this.CommandText = sql;
            return ExecuteCommitQuery(serializer);
        }

        public T ExecuteScalar<T>(string sql)
        {
            this.CommandText = sql;
            return ExecuteScalar<T>();
        }

        public IEnumerable<Commit> ExecuteCommitQuery(ISerialize serializer)
        {
            if (_conn.Trace)
            {
                Debug.WriteLine("Executing Query: " + this);
            }

            var stmt = Prepare();
            try
            {
                while (SQLite3.Step(stmt) == SQLite3.Result.Row)
                {

                    var headers = serializer.Deserialize<Dictionary<string, object>>(SQLite3.ColumnByteArray(stmt, HeadersIndex));
                    var events = serializer.Deserialize<List<EventMessage>>(SQLite3.ColumnByteArray(stmt, PayloadIndex));
                    yield return new Commit(
                        new Guid(SQLite3.ColumnString(stmt, StreamIdIndex)),
                        SQLite3.ColumnInt(stmt, StreamRevisionIndex),
                        new Guid(SQLite3.ColumnString(stmt, CommitIdIndex)),
                        SQLite3.ColumnInt(stmt, CommitSequenceIndex),
                        DateTime.Parse(SQLite3.ColumnString(stmt, CommitStampIndex)),
                        headers,
                        events);
                }
            }
            finally
            {
                SQLite3.Finalize(stmt);
            }
        }

        public IEnumerable<Snapshot> ExecuteSnapshotQuery(string sql, ISerialize serializer)
        {
            this.CommandText = sql;
            return ExecuteSnapshotQuery(serializer);
        }
        public IEnumerable<Snapshot> ExecuteSnapshotQuery(ISerialize serializer)
        {
            if (_conn.Trace)
            {
                Debug.WriteLine("Executing Query: " + this);
            }

            var stmt = Prepare();
            try
            {
                while (SQLite3.Step(stmt) == SQLite3.Result.Row)
                {

                    var payload = serializer.Deserialize<object>(SQLite3.ColumnByteArray(stmt, SnapshotPayloadIndex));
                    yield return new Snapshot(
                        new Guid(SQLite3.ColumnString(stmt, StreamIdIndex)),
                        SQLite3.ColumnInt(stmt, StreamRevisionIndex),
                        payload);

                }
            }
            finally
            {
                SQLite3.Finalize(stmt);
            }
        }

        public IEnumerable<StreamHead> ExecuteStreamHeadQuery(string sql)
        {
            this.CommandText = sql;
            return ExecuteStreamHeadQuery();
        }
        public IEnumerable<StreamHead> ExecuteStreamHeadQuery()
        {
            if (_conn.Trace)
            {
                Debug.WriteLine("Executing Query: " + this);
            }

            var stmt = Prepare();
            try
            {
                while (SQLite3.Step(stmt) == SQLite3.Result.Row)
                {

                    yield return new StreamHead(
                        new Guid(SQLite3.ColumnString(stmt, StreamIdIndex)),
                        SQLite3.ColumnInt(stmt, HeadRevisionIndex),
                        SQLite3.ColumnInt(stmt, SnapshotRevisionIndex));

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
                //Logger.Debug(Messages.ExceptionSuppressed);
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
