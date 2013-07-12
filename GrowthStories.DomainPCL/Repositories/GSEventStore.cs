using CommonDomain;
using CommonDomain.Persistence;
//using CommonDomain.Persistence.EventStore;
using EventStore;
using EventStore.Logging;
using EventStore.Persistence;
using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public class GSEventStore : OptimisticEventStore, IRebaseEvents
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

        public virtual IPersistSyncStreams MoreAdvanced
        {
            get { return this.Persistence; }
        }

        public void Rebase(Commit[] remove, Commit[] add)
        {

            foreach (var hook in this.PipelineHooks)
            {
                foreach (var attempt in add)
                {
                    //Logger.Debug(Resources.InvokingPreCommitHooks, attempt.CommitId, hook.GetType());
                    if (hook.PreCommit(attempt))
                        continue;

                    //Logger.Info(Resources.CommitRejectedByPipelineHook, hook.GetType(), attempt.CommitId);
                    return;
                }
            }

            Logger.Info("Rebasing with {0} commits to remove and {1} to add", remove.Length, add.Length - remove.Length);
            this.Persistence.Rebase(remove, add);

            foreach (var hook in this.PipelineHooks)
            {
                foreach (var attempt in add)
                {
                    hook.PostCommit(attempt);
                }
            }
        }
    }
}
