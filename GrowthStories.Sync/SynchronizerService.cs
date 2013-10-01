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
using Growthstories.Domain;

namespace Growthstories.Sync
{


    public class SynchronizerService : ISynchronizerService
    {

        public ITransportEvents Transporter { get; private set; }
        private readonly IRequestFactory RequestFactory;
        private readonly GSEventStore Persistence;
        private readonly IGSRepository Repository;

        private readonly IConstructSyncEventStreams StreamFactory;
        private ISyncPushRequest LastPushRequest;
        private readonly IUserService Context;
        //private AuthTokenService AuthService;

        public IList<ISyncPushRequest> PushRequests;
        public IList<ISyncPushResponse> PushResponses;
        public IList<ISyncPullRequest> PullRequests;
        public IList<ISyncPullResponse> PullResponses;

        public IList<ISyncCommunication> AllCommunication;

        public SynchronizerService(
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IGSRepository repository,
            IConstructSyncEventStreams streamFactory,
            IUserService ctx,
            IAuthTokenService authService
            )
        {
            Transporter = transporter;
            RequestFactory = requestFactory;
            Repository = repository;
            var r = repository as GSRepository;
            if (r != null)
                Persistence = r.EventStore;
            StreamFactory = streamFactory;
            Context = ctx;
        }

        //public ITransportEvents GetTransporter

        public ISyncPushRequest GetPushRequest()
        {
            if (this.LastPushRequest == null)
            {
                var pending = Pending().ToArray();
                if (pending.Length == 0)
                    return null;
                this.LastPushRequest = RequestFactory.CreatePushRequest(pending);

            }
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



        public void MarkSynchronized(ISyncPushRequest pushReq, ISyncPushResponse pushResp = null)
        {
            Guid? lastExecuted = null;
            if (pushResp != null && pushResp.StatusCode == 452 && pushResp.LastExecuted != default(Guid))
                lastExecuted = pushResp.LastExecuted;
            foreach (var stream in pushReq.Streams)
            {
                foreach (var commit in stream.Commits)
                {
                    bool mark = true;
                    if (lastExecuted.HasValue)
                    {
                        var match = commit.Events
                            .Select(x => (IEvent)x.Body)
                            .FirstOrDefault(x => x.EventId == lastExecuted.Value);
                        if (match != null)
                            mark = false;
                    }
                    if (mark)
                    {
                        Persistence.MoreAdvanced.MarkCommitAsSynchronized(commit);
                    }

                }
                if (stream.UncommittedRemoteEvents.Count > 0)
                {
                    stream.CommitRemoteChanges(Guid.NewGuid());
                    Repository.ClearAggregateFromCache(stream.StreamId);
                }

            }

        }

        public IEnumerable<ISyncEventStream> Pending()
        {
            foreach (var commits in Persistence.MoreAdvanced.GetUnsynchronizedCommits().GroupBy(x => x.StreamId))
            {

                yield return new SyncEventStream(commits, Persistence);
            }
        }



        //public void Synchronized(ISyncPushRequest pushReq)
        //{
        //    MarkSynchronized(pushReq);


        //}


        public Task TryAuth(ISyncPushRequest pushReq)
        {


            var UE = pushReq.EventsFromStreams().First(y => y is UserCreated && y.EntityId == Context.CurrentUser.Id) as UserCreated;




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
            this.AllCommunication = new List<ISyncCommunication>();
            this.LastPushRequest = null;

            return Task.Run<SyncResult>(async () =>
            {

                ISyncPushRequest pushReq = null;
                ISyncPullResponse pullResp = null;
                int cycles = 0;
                int mCycles = 3;
                do
                {
                    pushReq = GetPushRequest();
                    if (pushReq == null)
                        return new SyncResult()
                        {
                            Communication = this.AllCommunication
                        };

                    pullResp = await doCycle(pushReq, pullResp);

                    if (pullResp == null)
                    {
                        return new SyncResult()
                        {
                            Communication = this.AllCommunication
                        };
                    }

                    cycles++;
                } while (cycles <= mCycles);

                return new SyncResult();

            });

            //return await Agg.Handle((Synchronize)command, syncService);
        }




        private async Task<ISyncPullResponse> doCycle(ISyncPushRequest pushReq, ISyncPullResponse pullResp)
        {

            if (pullResp != null)
            {

            }

            this.PushRequests.Add(pushReq);
            this.AllCommunication.Add(pushReq);
            var pushResp = await Transporter.PushAsync(pushReq);
            this.PushResponses.Add(pushResp);
            this.AllCommunication.Add(pushResp);

            if (pushResp.StatusCode == 200) // OK
            {
                MarkSynchronized(pushReq, pushResp);
                //syncService.Synchronized(pushReq);
                try
                {
                    await TryAuth(pushReq);

                }
                catch
                {

                }
                //RaiseUserSynced(pushReq);
                //return null;
            }
            if (pushResp.StatusCode == 400) // BAD REQUEST
            {
                //return null;
            }
            if (pushResp.StatusCode == 452) // VERSION_TOO_LOW
            {
                MarkSynchronized(pushReq, pushResp);
                var pullReq = GetPullRequest();
                this.PullRequests.Add(pullReq);
                this.AllCommunication.Add(pullReq);
                var newPullResp = await Transporter.PullAsync(pullReq);
                this.PullResponses.Add(newPullResp);
                this.AllCommunication.Add(newPullResp);

                if (newPullResp.StatusCode == 200)
                {

                    List<ISyncEventStream> newStreams = new List<ISyncEventStream>();
                    foreach (var g in newPullResp.Events.OfType<EventBase>().GroupBy(x => x.StreamEntityId ?? x.EntityId))
                    {
                        ISyncEventStream match = pushReq.Streams.FirstOrDefault(x => x.StreamId == g.Key);
                        if (match == null)
                        {
                            match = new SyncEventStream(g.Key, this.Persistence);
                            newStreams.Add(match);
                        }
                        foreach (var e in g.OrderBy(y => y.EntityVersion))
                            match.AddRemote(e);
                    }

                    this.HandleRemoteStreams(newStreams);
                    return newPullResp;
                }



            }
            return null;


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
