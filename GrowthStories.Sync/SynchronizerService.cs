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


        public SynchronizerService(
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IPersistSyncStreams eventStore,
            IConstructSyncEventStreams streamFactory
            )
        {
            Transporter = transporter;
            RequestFactory = requestFactory;
            EventStore = eventStore;
            StreamFactory = streamFactory;
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

    }
}
