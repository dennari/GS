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
        private readonly IUIPersistence UIPersistence;

        public SynchronizerService(
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IGSRepository repository,
            GSEventStore store,
            IAggregateFactory factory,
            IMessageBus bus,
            IUserService authService,
            IUIPersistence UIPersistence
            )
        {
            Transporter = transporter;
            RequestFactory = requestFactory;
            Repository = repository;
            Store = store;
            Factory = factory;
            Bus = bus;
            this.AuthService = authService;
            this.UIPersistence = UIPersistence;
        }



        public Task<SyncResult> Synchronize(IGSApp app, ISyncPullRequest aPullReq = null, ISyncPushRequest aPushReq = null)
        {

            this.Pushes = new List<Tuple<ISyncPushRequest, ISyncPushResponse>>();
            this.Pulls = new List<Tuple<ISyncPullRequest, ISyncPullResponse>>();
            this.AllCommunication = new List<ISyncCommunication>();


            return Task.Run<SyncResult>(async () =>
            {

                if (Transporter.AuthToken == null)
                    await AuthService.AuthorizeUser();

                ISyncPushRequest pushReq = aPushReq ?? RequestFactory.CreatePushRequest();
                ISyncPullRequest pullReq = aPullReq ?? RequestFactory.CreatePullRequest(app.State.SyncStreams.ToArray());
                ISyncPullResponse pullResp = null;
                ISyncPushResponse pushResp = null;

                int cycles = 0;
                int mCycles = 1;
                do
                {

                    var R = await doCycle(app, pullReq, pushReq);
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
                } while (cycles < mCycles);

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


        private async Task<Tuple<ISyncPullResponse, ISyncPushResponse>> doCycle(IGSApp app, ISyncPullRequest pullReq, ISyncPushRequest pushReq)
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
                        this.HandleRemoteStreams(app, pullResp, pushReq);

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

        private void HandleRemoteStreams(IGSApp app, ISyncPullResponse resp, ISyncPushRequest req)
        {
            var syncStampCommands = new List<SetSyncStamp>();

            foreach (var stream in resp.Streams)
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
                    var first = stream.UncommittedRemoteEvents.First();
                    var cFirst = first as ICreateEvent;

                    if (cFirst == null)
                    {
                        if (stream.UncommittedRemoteEvents.Count == 1 && IsRelationshipNotification(first))
                        {
                            this.HandleRelationshipNotification(first);
                            continue;
                        }
                        else
                            throw err;
                    }

                    aggregate = Factory.Build(cFirst.AggregateType);
                }

                var mergeFlag = false;
                var duplicates = new HashSet<Guid>();
                foreach (var e in stream.UncommittedRemoteEvents)
                {
                    try
                    {
                        aggregate.ApplyRemoteEvent(e);
                    }
                    catch (DomainError err2)
                    {

                        if (err2.Name == "duplicate_event")
                        {
                            Logger.Info(err2.Message);
                            duplicates.Add(e.MessageId);
                        }
                        else if (err2.Name == "version_mismatch")
                        {
                            Merge(aggregate, stream, req, duplicates);
                            mergeFlag = true;
                            break;
                        }
                        else
                            throw err2;

                    }
                }


                if (mergeFlag) // retry
                {
                    foreach (var e in stream.UncommittedRemoteEvents)
                    {
                        try
                        {
                            aggregate.ApplyRemoteEvent(e);
                        }
                        catch (DomainError err2)
                        {

                            if (err2.Name == "duplicate_event")
                                Logger.Info(err2.Message);
                            else if (err2.Name == "version_mismatch")
                                throw err2;
                            else
                                throw err2;

                        }
                    }
                }





                Repository.SaveRemote(aggregate, resp.SyncStamp);
                if (aggregate.SyncStreamType != StreamType.NULL)
                    syncStampCommands.Add(new SetSyncStamp(aggregate.Id, resp.SyncStamp));


            }


            if (syncStampCommands.Count > 0)
            {
                foreach (var cmd in syncStampCommands)
                    app.Handle(cmd);
                Repository.Save(app);
            }

        }

        private void Merge(IGSAggregate aggregate, ISyncEventStream remote, ISyncPushRequest req, ISet<Guid> duplicates)
        {
            ISyncEventStream local = null;
            if (req != null && (local = req.Streams.FirstOrDefault(x => x.StreamId == remote.StreamId)) != null)
            {

                aggregate.Resolve(
                    local.CommittedEvents.Select(x => (IEvent)x.Body).ToArray(),
                    remote.UncommittedRemoteEvents.ToArray(),
                    duplicates
                 );

                var httpreq = req as HttpPushRequest;
                if (httpreq != null)
                    httpreq.Retranslate();

            }
        }

        //private void Merge(ISyncEventStream local, ISyncEventStream remote)
        //{

        //}

        private void HandleRelationshipNotification(IEvent notification)
        {
            var req = notification as CollaborationRequested;
            if (req != null)
            {
                var u = (User)Repository.GetById(req.Target);

                if (u.State.Collaborators.Contains(req.AggregateId))
                {
                    UIPersistence.SaveCollaborator(req.AggregateId, true);
                }

            }
            var req2 = notification as CollaborationDenied;
            if (req2 != null)
            {
                UIPersistence.SaveCollaborator(req.AggregateId, false);
            }
            //var req3 = notification as BecameFollower;
            //if (req3 != null)
            //{
            //    UIPersistence.SaveCollaborator(req.AggregateId, false);
            //}

        }








    }
}
