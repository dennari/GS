using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public class SynchronizerCommandHandler : CommandHandlerBase, IAsyncCommandHandler<Synchronize>
    {
        private readonly ISynchronizerService SyncService;
        private readonly IPersistSyncStreams Persistence;

        public IList<ISyncPushRequest> PushRequests;
        public IList<ISyncPushResponse> PushResponses;
        public IList<ISyncPullRequest> PullRequests;
        public IList<ISyncPullResponse> PullResponses;

        public SynchronizerCommandHandler(ISynchronizerService service, IPersistSyncStreams persistence)
        {
            this.SyncService = service;
            this.Persistence = persistence;
        }

        public Task HandleAsync(Synchronize command)
        {
            this.PushRequests = new List<ISyncPushRequest>();
            this.PushResponses = new List<ISyncPushResponse>();
            this.PullRequests = new List<ISyncPullRequest>();
            this.PullResponses = new List<ISyncPullResponse>();
            return Task.Run(async () =>
            {

                //ISyncPushRequest pushReq = null;
                //ISyncPullResponse pullResp = null;
                int cycles = 0;
                int mCycles = 3;
                do
                {
                    var pushReq = SyncService.GetPushRequest();
                    if (pushReq == null)
                        return;

                    var pullResp = await doCycle(pushReq, SyncService);

                    if (pullResp == null)
                    {
                        return;
                    }
                    else
                    {
                        HandleRemoteStreams(pullResp.Streams);
                    }

                    cycles++;
                } while (cycles <= mCycles);


            });

            //return await Agg.Handle((Synchronize)command, syncService);
        }




        private async Task<ISyncPullResponse> doCycle(ISyncPushRequest pushReq, ISynchronizerService syncService)
        {

            this.PushRequests.Add(pushReq);
            var pushResp = await syncService.Transporter.PushAsync(pushReq);
            this.PushResponses.Add(pushResp);
            if (pushResp.StatusCode == 200)
            {
                syncService.MarkAllSynchronized(pushReq);
                //syncService.Synchronized(pushReq);
                await syncService.TryAuth(pushReq);
                //RaiseUserSynced(pushReq);
                return null;
            }

            var pullReq = syncService.GetPullRequest();
            this.PullRequests.Add(pullReq);
            var pullResp = await syncService.Transporter.PullAsync(pullReq);
            this.PullResponses.Add(pullResp);
            return pullResp;


        }

        private void HandleRemoteStreams(IEnumerable<ISyncEventStream> remoteStreams)
        {
            foreach (var stream in remoteStreams)
            {
                stream.CommitChanges(Guid.NewGuid());
            }
        }



        //public IDictionary<Tuple<Type, Type>, Action<IGSAggregate, IEntityCommand>> RegisterHandlers()
        //{
        //    return null;
        //}

        //public IDictionary<Tuple<Type, Type>, Func<IGSAggregate, IEntityCommand, Task<object>>> RegisterAsyncHandlers()
        //{
        //    return new Dictionary<Tuple<Type, Type>, Func<IGSAggregate, IEntityCommand, Task<object>>>()
        //    {
        //        {Tuple.Create(typeof(Synchronizer),typeof(Synchronize)),this.Synchronize}
        //    };
        //}

    }
}
