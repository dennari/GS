namespace Growthstories.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    //using Logging;
    using EventStore.Persistence;
    using EventStore;


    public class SyncAwarePersistenceDecorator : PipelineHooksAwarePersistanceDecorator, IPersistSyncStreams
    {
        //private static readonly ILog Logger = LogFactory.BuildLogger(typeof (PipelineHooksAwarePersistanceDecorator));
        private readonly IPersistSyncStreams original;

        public bool IsRemoteCommit { get; set; }

        public SyncAwarePersistenceDecorator(IPersistSyncStreams original, IEnumerable<IPipelineHook> pipelineHooks)
            : base(original, pipelineHooks)
        {

            this.original = original;

        }

        public new void Commit(Commit attempt)
        {
            if (IsRemoteCommit)
                original.Commit(new GSCommit(attempt)
                {
                    IsRemoteCommit = true
                });
        }

        public IEnumerable<GSCommit> GetUnsynchronizedCommits(int globalSequence)
        {
            return original.GetUnsynchronizedCommits(globalSequence);
        }

        public int GetGlobalCommitSequence()
        {
            return original.GetGlobalCommitSequence();
        }

        public void RunInTransaction(Action action)
        {
            original.RunInTransaction(action);
        }
    }
}