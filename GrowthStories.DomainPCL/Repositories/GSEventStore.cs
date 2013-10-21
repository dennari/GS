using CommonDomain;
using CommonDomain.Persistence;
//using CommonDomain.Persistence.EventStore;
using EventStore;
using EventStore.Logging;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public class GSEventStore : OptimisticEventStore
    {
        private readonly IPersistSyncStreams Persistence;
        private readonly IEnumerable<IPipelineHook> PipelineHooks;

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(GSEventStore));

        public GSEventStore(IPersistSyncStreams persistence, IEnumerable<IPipelineHook> pipelineHooks)
            : base(persistence, pipelineHooks)
        {
            this.Persistence = persistence;
            this.PipelineHooks = pipelineHooks;
        }

        //public override IEventStream CreateStream(Guid streamId)
        //{
        //    //Logger.Info(Resources.CreatingStream, streamId);
        //    return new SyncEventStream(streamId, this, Persistence);
        //}

        //public override IEventStream OpenStream(Guid streamId, int minRevision, int maxRevision)
        //{
        //    maxRevision = maxRevision <= 0 ? int.MaxValue : maxRevision;

        //    //Logger.Debug(Resources.OpeningStreamAtRevision, streamId, minRevision, maxRevision);
        //    return new SyncEventStream(streamId, this, minRevision, maxRevision, this.Persistence);
        //}
        //public override IEventStream OpenStream(Snapshot snapshot, int maxRevision)
        //{
        //    if (snapshot == null)
        //        throw new ArgumentNullException("snapshot");

        //    //Logger.Debug(Resources.OpeningStreamWithSnapshot, snapshot.StreamId, snapshot.StreamRevision, maxRevision);
        //    maxRevision = maxRevision <= 0 ? int.MaxValue : maxRevision;
        //    return new SyncEventStream(snapshot, this, maxRevision, this.Persistence);
        //}


        //public virtual IPersistSyncStreams MoreAdvanced
        //{
        //    get { return this.Persistence; }
        //}

        //public void Rebase(Commit[] remove, Commit[] add)
        //{

        //    foreach (var hook in this.PipelineHooks)
        //    {
        //        foreach (var attempt in add)
        //        {
        //            //Logger.Debug(Resources.InvokingPreCommitHooks, attempt.CommitId, hook.GetType());
        //            if (hook.PreCommit(attempt))
        //                continue;

        //            //Logger.Info(Resources.CommitRejectedByPipelineHook, hook.GetType(), attempt.CommitId);
        //            return;
        //        }
        //    }

        //    Logger.Info("Rebasing with {0} commits to remove and {1} to add", remove.Length, add.Length - remove.Length);
        //    this.Persistence.Rebase(remove, add);

        //    foreach (var hook in this.PipelineHooks)
        //    {
        //        foreach (var attempt in add)
        //        {
        //            hook.PostCommit(attempt);
        //        }
        //    }
        //}
    }
}
