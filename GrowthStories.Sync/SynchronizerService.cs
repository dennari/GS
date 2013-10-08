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
        private readonly IGSRepository Repository;
        private readonly IRequestFactory RequestFactory;

        private IList<Tuple<ISyncPushRequest, ISyncPushResponse>> Pushes;
        private IList<Tuple<ISyncPullRequest, ISyncPullResponse>> Pulls;

        public IList<ISyncCommunication> AllCommunication;

        public SynchronizerService(ITransportEvents transporter, IRequestFactory requestFactory, IGSRepository repository)
        {
            Transporter = transporter;
            RequestFactory = requestFactory;
            Repository = repository;
        }



        public Task<SyncResult> Synchronize()
        {

            this.Pushes = new List<Tuple<ISyncPushRequest, ISyncPushResponse>>();
            this.Pulls = new List<Tuple<ISyncPullRequest, ISyncPullResponse>>();
            this.AllCommunication = new List<ISyncCommunication>();


            return Task.Run<SyncResult>(async () =>
            {

                ISyncPushRequest pushReq = null;
                ISyncPullRequest pullReq = null;
                ISyncPullResponse pullResp = null;
                ISyncPushResponse pushResp = null;

                pullReq = RequestFactory.CreatePullRequest();
                pushReq = RequestFactory.CreatePushRequest();

                int cycles = 0;
                int mCycles = 3;
                do
                {

                    var R = await doCycle(pullReq, pushReq);
                    pullResp = R.Item1;
                    pushResp = R.Item2;



                    // update stats
                    this.Pushes.Add(Tuple.Create(pushReq, pushResp));
                    this.Pulls.Add(Tuple.Create(pullReq, pullResp));
                    this.AllCommunication.Add(pullReq);
                    this.AllCommunication.Add(pullResp);
                    this.AllCommunication.Add(pushReq);
                    this.AllCommunication.Add(pullResp);


                    if (pushResp == null) // there's nothing to push
                    {
                        break;
                    }
                    else
                    {
                        if (pushResp.StatusCode == GSStatusCode.OK)
                            break;
                    }



                    cycles++;
                } while (cycles <= mCycles);

                return new SyncResult()
                {
                    Communication = this.AllCommunication,
                    Pushes = this.Pushes,
                    Pulls = this.Pulls
                };

            });

            //return await Agg.Handle((Synchronize)command, syncService);
        }




        private async Task<Tuple<ISyncPullResponse, ISyncPushResponse>> doCycle(ISyncPullRequest pullReq, ISyncPushRequest pushReq)
        {

            ISyncPullResponse pullResp = null;

            if (pullReq.Streams.Count > 0)
            {
                pullResp = await Transporter.PullAsync(pullReq);





                if (pullResp.StatusCode == GSStatusCode.OK)
                {
                    //var newStreams = RequestFactory.MatchStreams(pullResp, pushReq);
                    //if (newStreams.Count > 0)
                    //    throw new InvalidOperationException("Extraneous streams present in the pull response.");
                    //foreach (var stream in pullResp.Streams)
                    //    stream.CommitRemoteChanges(Guid.NewGuid());
                    //return pullResp;
                    this.HandleRemoteStreams(pullResp);

                }

            }
            ISyncPushResponse pushResp = null;

            if (pushReq.Streams.Count > 0)
            {
                pushResp = await Transporter.PushAsync(pushReq);



                if (pushResp.StatusCode == GSStatusCode.OK) // OK
                {
                    foreach (var stream in pushReq.Streams)
                        stream.MarkCommitsSynchronized();

                }
                if (pushResp.StatusCode == GSStatusCode.BAD_REQUEST) // BAD REQUEST
                {
                    //return null;
                }
                if (pushResp.StatusCode == GSStatusCode.VERSION_TOO_LOW) // VERSION_TOO_LOW
                {
                    foreach (var stream in pushReq.Streams)
                        stream.MarkCommitsSynchronized(pushResp);
                }
            }
            return Tuple.Create(pullResp, pushResp);


        }

        private void HandleRemoteStreams(ISyncPullResponse r)
        {
            foreach (var stream in r.Streams)
            {
                var aggregate = Repository.GetById(stream.StreamId);
                foreach (var e in stream.UncommittedRemoteEvents)
                    aggregate.ApplyRemoteEvent(e);
                Repository.SaveRemote(aggregate, r.SyncStamp);

            }
        }








    }
}
