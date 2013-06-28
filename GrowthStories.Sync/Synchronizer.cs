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
    public class Synchronizer
    {

        private readonly ITransportEvents Transporter;
        private readonly IRequestFactory RequestFactory;
        private readonly IPersistSyncStreams EventStore;


        public Synchronizer(
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IPersistSyncStreams eventStore
            )
        {
            Transporter = transporter;
            RequestFactory = requestFactory;
            EventStore = eventStore;
        }


        public async Task<IList<ISyncRequest>> Synchronize()
        {


            ISyncPushRequest pushReq = GetPushRequest();
            if (pushReq == null)
                return null;

            var r = new List<ISyncRequest>() { pushReq };
            int MaxTries = 5;
            int Counter = 0;

            ISyncPushResponse pushResp;
            ISyncPullRequest pullReq;
            ISyncPullResponse pullResp;

            do
            {

                // push
                pushResp = await Transporter.PushAsync(pushReq);
                Counter++;

                if (pushResp.StatusCode == 200)
                {
                    foreach (var stream in pushReq.Streams)
                        foreach (var commit in stream.Commits)
                            EventStore.MarkCommitAsDispatched(commit);
                    break;
                }

                // pull
                pullReq = RequestFactory.CreatePullRequest(pushReq.Streams);
                r.Add(pullReq);
                pullResp = await Transporter.PullAsync(pullReq);

                // save
                foreach (var remote in pullResp.Streams.Except(pushReq.Streams))
                {
                    remote.CommitPullChanges(Guid.NewGuid());
                }

                // rebase
                var pairs = from local in pushReq.Streams
                            join remote in pullResp.Streams on local.StreamId equals remote.StreamId
                            select Tuple.Create(local, remote);

                foreach (var p in pairs)
                {
                    p.Item1.Rebase(p.Item2);
                }

                pushReq = GetPushRequest();
                r.Add(pushReq);

            } while (Counter < MaxTries);



            return r;
        }

        private ISyncPushRequest GetPushRequest()
        {
            var pending = Pending().ToArray();
            if (pending.Length == 0)
                return null;

            return RequestFactory.CreatePushRequest(pending);
        }

        public IEnumerable<ISyncEventStream> Pending()
        {
            foreach (var commits in EventStore.GetUndispatchedCommits().GroupBy(x => x.StreamId))
            {

                yield return new SyncEventStream(commits, EventStore);
            }
        }

    }
}
