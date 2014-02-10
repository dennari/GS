namespace GrowthStories.Core
{
    using System;

    using EventStore.Dispatcher;
    using EventStore.Logging;
    using EventStore.Persistence;
    using EventStore;

    public class GSDispatchScheduler : IScheduleDispatches
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(GSDispatchScheduler));
        private readonly IDispatchCommits dispatcher;
        private readonly IPersistStreams persistence;
        private bool disposed;

        public GSDispatchScheduler(IDispatchCommits dispatcher, IPersistStreams persistence)
        {
            this.dispatcher = dispatcher;
            this.persistence = persistence;

            //this.Start();
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

            this.disposed = true;
            this.dispatcher.Dispose();
            this.persistence.Dispose();
        }

        private bool _Started = false;
        protected virtual void Start()
        {

            if (_Started)
                return;

            _Started = true;
            //this.persistence.Initialize();

            foreach (var commit in this.persistence.GetUndispatchedCommits())
                this.ScheduleDispatch(commit);
        }

        public virtual void ScheduleDispatch(Commit commit)
        {
            Start();
            this.DispatchImmediately(commit);
            this.MarkAsDispatched(commit);
        }
        private void DispatchImmediately(Commit commit)
        {
            try
            {
                this.dispatcher.Dispatch(commit);
            }
            catch
            {
                throw;
            }
        }
        private void MarkAsDispatched(Commit commit)
        {
            try
            {
                this.persistence.MarkCommitAsDispatched(commit);
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}