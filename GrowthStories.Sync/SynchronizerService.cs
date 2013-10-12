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
using ReactiveUI;
using EventStore.Logging;

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
        private readonly GSEventStore Store;
        private readonly IAggregateFactory Factory;
        private readonly IMessageBus Bus;

        private static ILog Logger = LogFactory.BuildLogger(typeof(SynchronizerService));
        private IUserService AuthService;

        public SynchronizerService(
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IGSRepository repository,
            GSEventStore store,
            IAggregateFactory factory,
            IMessageBus bus,
            IUserService authService
            )
        {
            Transporter = transporter;
            RequestFactory = requestFactory;
            Repository = repository;
            Store = store;
            Factory = factory;
            Bus = bus;
            this.AuthService = authService;
        }



        public Task<SyncResult> Synchronize(ISyncPullRequest aPullReq, ISyncPushRequest aPushReq = null)
        {

            this.Pushes = new List<Tuple<ISyncPushRequest, ISyncPushResponse>>();
            this.Pulls = new List<Tuple<ISyncPullRequest, ISyncPullResponse>>();
            this.AllCommunication = new List<ISyncCommunication>();


            return Task.Run<SyncResult>(async () =>
            {

                if (Transporter.AuthToken == null)
                    await AuthService.AuthorizeUser();

                ISyncPushRequest pushReq = aPushReq ?? RequestFactory.CreatePushRequest();
                ISyncPullRequest pullReq = aPullReq;
                ISyncPullResponse pullResp = null;
                ISyncPushResponse pushResp = null;

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

        public async Task<bool> CreateUserAsync(Guid userId)
        {
            var userStream = (SyncEventStream)Store.OpenStream(userId, 0, 1);
            var pushReq = RequestFactory.CreatePushRequest(new ISyncEventStream[] { userStream });
            var pushResp = await Transporter.PushAsync(pushReq);
            if (pushResp.StatusCode != GSStatusCode.OK)
                return false;
            userStream.MarkCommitsSynchronized();
            return true;
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
                    if (pullResp.Streams != null)
                        this.HandleRemoteStreams(pullResp);

                }

            }
            ISyncPushResponse pushResp = null;

            if (!pushReq.IsEmpty)
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


        private bool IsRelationshipNotification(IEvent e)
        {
            if (e is BecameFollower)
                return true;
            if (e is CollaborationRequested)
                return true;
            if (e is CollaborationDenied)
                return true;
            return false;
        }

        private void HandleRemoteStreams(ISyncPullResponse r)
        {

            foreach (var stream in r.Streams)
            {

                if (stream.UncommittedRemoteEvents.Count == 0)
                    continue;
                if (stream.StreamId == UserState.UnregUserId)
                    continue;
                IGSAggregate aggregate = null;
                try
                {
                    aggregate = Repository.GetById(stream.StreamId);
                }
                catch (DomainError err)
                {
                    var first = stream.UncommittedRemoteEvents.First() as ICreateEvent;
                    if (first == null)
                    {
                        if (stream.UncommittedRemoteEvents.Count == 1 && IsRelationshipNotification(stream.UncommittedRemoteEvents.First()))
                        {
                            this.HandleReadModelStream(stream);
                            continue;
                        }
                        else
                            throw err;
                    }

                    aggregate = Factory.Build(first.AggregateType);
                }

                foreach (var e in stream.UncommittedRemoteEvents)
                {
                    try
                    {
                        aggregate.ApplyRemoteEvent(e);
                    }
                    catch (DomainError err2)
                    {

                        if (err2.Name != "duplicate_event")
                            throw err2;
                        Logger.Info(err2.Message);
                    }
                }

                Repository.SaveRemote(aggregate, r.SyncStamp);
                if (aggregate.SyncStreamType != StreamType.NULL)
                    Bus.SendCommand(new SetSyncStamp(aggregate.Id, r.SyncStamp));

            }
        }

        private void HandleReadModelStream(ISyncEventStream stream)
        {
            throw new NotImplementedException();
        }








    }
}
