namespace Growthstories.Sync
{
    using EventStore;
    using EventStore.Logging;
    using EventStore.Persistence;
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public class GSEventStore : OptimisticEventStore
    {
        public bool IsRemoteCommit { get; set; }


        public GSEventStore(IPersistStreams persistence, IEnumerable<IPipelineHook> pipelineHooks)
            : base(persistence, pipelineHooks)
        {

        }

        public override void Commit(Commit attempt)
        {
            Commit N = attempt;
            if (IsRemoteCommit)
                N = new GSCommit(attempt) { IsRemoteCommit = true };
            base.Commit(N);
        }

    }

}