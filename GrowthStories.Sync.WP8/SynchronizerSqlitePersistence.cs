using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    class SynchronizerSqlitePersistence : IStoreSyncHeads, IDisposable
    {
        private readonly string Path;
        private bool disposed = false;

        public void MarkPublic(Guid StreamId)
        {
            throw new NotImplementedException();
        }

        public void MarkPrivate(Guid StreamId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SyncHead> GetSyncHeads()
        {
            throw new NotImplementedException();
        }

        public bool PersistSyncHead(SyncHead head)
        {
            throw new NotImplementedException();
        }


        protected virtual IEnumerable<T> ExecuteQuery<T>(Guid streamId, Func<SQLiteCommand, IEnumerable<T>> query)
        {
            this.ThrowWhenDisposed();

            //var scope = this.OpenQueryScope();
            SQLiteConnection connection = null;
            //IDbTransaction transaction = null;
            SQLiteCommand statement = null;

            try
            {
                connection = new SQLiteConnection(Path);
                connection.Trace = true;
                //transaction = this.dialect.OpenTransaction(connection);
                statement = connection.CreateCommand("");

                //statement.PageSize = this.pageSize;

                //Logger.Verbose(Messages.ExecutingQuery);
                return query(statement);
            }
            catch (Exception e)
            {
                if (statement != null)
                    statement.Dispose();
                //if (transaction != null)
                //    transaction.Dispose();
                if (connection != null)
                    connection.Dispose();
                //if (scope != null)
                //    scope.Dispose();

                //Logger.Debug(Messages.StorageThrewException, e.GetType());
                if (e is StorageUnavailableException)
                    throw;

                throw new StorageException(e.Message, e);
            }
        }

        private void ThrowWhenDisposed()
        {
            if (!this.disposed)
                return;

            //Logger.Warn(Messages.AlreadyDisposed);
            throw new ObjectDisposedException("Already disposed");
        }

        protected virtual T ExecuteCommand<T>(Guid streamId, Func<SQLiteCommand, T> command)
        {
            this.ThrowWhenDisposed();

            //using (var scope = this.OpenCommandScope())
            using (var connection = new SQLiteConnection(Path))
            using (var statement = connection.CreateCommand(""))
            {
                connection.Trace = true;
                try
                {
                    //Logger.Verbose(Messages.ExecutingCommand);
                    var rowsAffected = command(statement);
                    //Logger.Verbose(Messages.CommandExecuted, rowsAffected);

                    //if (transaction != null)
                    //    transaction.Commit();

                    //if (scope != null)
                    //    scope.Complete();

                    return rowsAffected;
                }
                catch (Exception e)
                {
                    //Logger.Debug(Messages.StorageThrewException, e.GetType());
                    //if (!RecoverableException(e))
                    throw new StorageException(e.Message, e);

                    //Logger.Info(Messages.RecoverableExceptionCompletesScope);
                    //if (scope != null)
                    //   scope.Complete();

                    //throw;
                }
            }
        }



        public void Dispose()
        {
            disposed = true;
        }
    }
}
