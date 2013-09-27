using System;
using System.Linq;
using CommonDomain.Core;
using CommonDomain;
using EventStore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Growthstories.Domain.Entities;
using Growthstories.Core;
using System.Net.Http;
using Growthstories.Domain.Messaging;
using EventStore.Persistence;

namespace Growthstories.Sync
{


    public class SynchronizerService : ISynchronizerService
    {

        public ITransportEvents Transporter { get; private set; }
        private readonly IRequestFactory RequestFactory;
        private readonly IPersistSyncStreams EventStore;
        private readonly IConstructSyncEventStreams StreamFactory;
        private ISyncPushRequest LastPushRequest;
        private readonly IUserService Context;
        //private AuthTokenService AuthService;

        public IList<ISyncPushRequest> PushRequests;
        public IList<ISyncPushResponse> PushResponses;
        public IList<ISyncPullRequest> PullRequests;
        public IList<ISyncPullResponse> PullResponses;


        public SynchronizerService(
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IPersistSyncStreams eventStore,
            IConstructSyncEventStreams streamFactory,
            IUserService ctx,
            IAuthTokenService authService
            )
        {
            Transporter = transporter;
            RequestFactory = requestFactory;
            EventStore = eventStore;
            StreamFactory = streamFactory;
            Context = ctx;
        }

        //public ITransportEvents GetTransporter

        public ISyncPushRequest GetPushRequest()
        {
            var pending = Pending().ToArray();
            if (pending.Length == 0)
                return null;
            this.LastPushRequest = RequestFactory.CreatePushRequest(pending);
            return this.LastPushRequest;
        }

        public ISyncPullRequest GetPullRequest()
        {
            if (this.LastPushRequest == null)
                throw new InvalidOperationException("No push requests registered");

            return RequestFactory.CreatePullRequest(this.LastPushRequest.Streams);
        }

        //public IEnumerable<RebasePair> MatchStreams(ISyncPushRequest pushReq, ISyncPullResponse pullResp)
        //{
        //    // save
        //    //foreach (var remote in pullResp.Streams.Except(pushReq.Streams))
        //    //{
        //    //    remote.CommitPullChanges(Guid.NewGuid());
        //    //}

        //    // rebase
        //    if (pushReq == null)
        //        throw new ArgumentNullException("pushReq");
        //    if (pullResp == null)
        //        throw new ArgumentNullException("pullResp");

        //    return from remote in pullResp.Streams
        //           select new RebasePair()
        //           {
        //               Remote = remote,
        //               Local = (from local in pushReq.Streams where local.StreamId == remote.StreamId select local).FirstOrDefault()
        //           };

        //}



        public void MarkAllSynchronized(ISyncPushRequest pushReq)
        {
            foreach (var stream in pushReq.Streams)
                foreach (var commit in stream.Commits)
                    EventStore.MarkCommitAsSynchronized(commit);

        }

        public IEnumerable<ISyncEventStream> Pending()
        {
            foreach (var commits in EventStore.GetUnsynchronizedCommits().GroupBy(x => x.StreamId))
            {

                yield return this.StreamFactory.CreateStream(commits);
            }
        }



        public void Synchronized(ISyncPushRequest pushReq)
        {
            MarkAllSynchronized(pushReq);


        }


        public Task TryAuth(ISyncPushRequest pushReq)
        {

            try
            {
                var UE = pushReq.EventsFromStreams().First(y => y is UserCreated && y.EntityId == Context.CurrentUser.Id) as UserCreated;

                //RaiseEvent(new UserSynchronized(this.Id, UE.EntityId, UE.Username, UE.Password, UE.Email));

            }
            catch (Exception)
            {
                return null;
            }

            return Task.Run(async () =>
            {

                await Context.TryAuth();
            });

        }





        public Task<SyncResult> Synchronize()
        {
            this.PushRequests = new List<ISyncPushRequest>();
            this.PushResponses = new List<ISyncPushResponse>();
            this.PullRequests = new List<ISyncPullRequest>();
            this.PullResponses = new List<ISyncPullResponse>();
            return Task.Run<SyncResult>(async () =>
            {

                //ISyncPushRequest pushReq = null;
                //ISyncPullResponse pullResp = null;
                int cycles = 0;
                int mCycles = 3;
                do
                {
                    var pushReq = GetPushRequest();
                    if (pushReq == null)
                        return new SyncResult();

                    var pullResp = await doCycle(pushReq);

                    if (pullResp == null)
                    {
                        return new SyncResult();
                    }
                    else
                    {
                        HandleRemoteStreams(pullResp.Streams);
                    }

                    cycles++;
                } while (cycles <= mCycles);

                return new SyncResult();

            });

            //return await Agg.Handle((Synchronize)command, syncService);
        }




        private async Task<ISyncPullResponse> doCycle(ISyncPushRequest pushReq)
        {

            this.PushRequests.Add(pushReq);
            var pushResp = await Transporter.PushAsync(pushReq);
            this.PushResponses.Add(pushResp);
            if (pushResp.StatusCode == 200)
            {
                MarkAllSynchronized(pushReq);
                //syncService.Synchronized(pushReq);
                await TryAuth(pushReq);
                //RaiseUserSynced(pushReq);
                return null;
            }

            var pullReq = GetPullRequest();
            this.PullRequests.Add(pullReq);
            var pullResp = await Transporter.PullAsync(pullReq);
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














    }
}
