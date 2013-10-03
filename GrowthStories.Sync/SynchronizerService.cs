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
using System.Net;

namespace Growthstories.Sync
{


    public class SynchronizerService : ISynchronizerService
    {

        public ITransportEvents Transporter { get; private set; }
        private readonly IRequestFactory RequestFactory;
        private readonly GSEventStore Persistence;
        private readonly IGSRepository Repository;

        private ISyncPushRequest LastPushRequest;
        //private AuthTokenService AuthService;

        public IList<ISyncPushRequest> PushRequests;
        public IList<ISyncPushResponse> PushResponses;
        public IList<ISyncPullRequest> PullRequests;
        public IList<ISyncPullResponse> PullResponses;

        public IList<ISyncCommunication> AllCommunication;

        public SynchronizerService(
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IGSRepository repository
            )
        {
            Transporter = transporter;
            RequestFactory = requestFactory;
            Repository = repository;
            var r = repository as GSRepository;
            if (r != null)
                Persistence = r.EventStore;
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

        public ISyncPullRequest GetRemoteStreamsRequest(IEnumerable<Guid> streamIds)
        {
            return RequestFactory.CreatePullRequest(streamIds.Select(x => new SyncEventStream(x, this.Persistence)));
        }

        public void MarkSynchronized(ISyncPushRequest pushReq, ISyncPushResponse pushResp = null)
        {
            Guid? lastExecuted = null;
            if (pushResp != null && pushResp.StatusCode == GSStatusCode.VERSION_TOO_LOW && pushResp.LastExecuted != default(Guid))
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




        private async Task<ISyncPullResponse> doCycle(ISyncPushRequest pushReq, ISyncPullResponse prevPullResp)
        {


            this.PushRequests.Add(pushReq);
            this.AllCommunication.Add(pushReq);
            var pushResp = await Transporter.PushAsync(pushReq);
            this.PushResponses.Add(pushResp);
            this.AllCommunication.Add(pushResp);

            if (pushResp.StatusCode == GSStatusCode.OK) // OK
            {
                MarkSynchronized(pushReq, pushResp);

            }
            if (pushResp.StatusCode == GSStatusCode.BAD_REQUEST) // BAD REQUEST
            {
                //return null;
            }
            if (pushResp.StatusCode == GSStatusCode.VERSION_TOO_LOW) // VERSION_TOO_LOW
            {
                MarkSynchronized(pushReq, pushResp);
                var pullReq = RequestFactory.CreatePullRequest(pushReq.Streams);
                this.PullRequests.Add(pullReq);
                this.AllCommunication.Add(pullReq);

                var pullResp = await Transporter.PullAsync(pullReq);
                this.PullResponses.Add(pullResp);
                this.AllCommunication.Add(pullResp);

                if (pullResp.StatusCode == GSStatusCode.OK)
                {
                    var newStreams = MatchStreams(pullResp, pushReq);
                    if (newStreams.Count > 0)
                        throw new InvalidOperationException("Extraneous streams present in the pull response.");
                    return pullResp;
                }


            }
            return null;


        }



        public List<ISyncEventStream> MatchStreams(ISyncPullResponse resp, ISyncRequest req)
        {
            List<ISyncEventStream> unmatched = new List<ISyncEventStream>();
            foreach (var g in resp.Events)
            {
                ISyncEventStream match = req.Streams.FirstOrDefault(x => x.StreamId == g.Key);
                if (match == null)
                {
                    match = new SyncEventStream(g.Key, this.Persistence);
                    unmatched.Add(match);
                }
                foreach (var e in g.OrderBy(y => y.EntityVersion))
                    match.AddRemote(e);
            }
            return unmatched;
        }




    }
}
