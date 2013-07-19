using System;
using System.Linq;

using NUnit.Framework;
using Growthstories.Domain.Messaging;
using Ninject;
using Growthstories.Core;
using CommonDomain.Persistence;
using Growthstories.Sync;
using Ninject.Parameters;
using SimpleTesting;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.Collections.Generic;
using Growthstories.Domain.Entities;
using CommonDomain;
using EventStore;

using CommonDomain.Persistence.EventStore;
using EventStore.Dispatcher;
using Growthstories.Domain;
using EventStore.Logging;

namespace Growthstories.DomainTests
{
    public class SyncTest
    {
        IAuthUser Ctx;
        IKernel kernel;
        [SetUp]
        public void SetUp()
        {
            if (kernel != null)
                kernel.Dispose();
            kernel = new StandardKernel(new TestModule());
            Ctx = CurrentUser;
        }
        private ILog Log = new LogTo4Net(typeof(SyncTest));

        public T Get<T>() { return kernel.Get<T>(); }
        public IDispatchCommands Handler { get { return Get<IDispatchCommands>(); } }
        public SynchronizerCommandHandler SyncHandler { get { return Get<SynchronizerCommandHandler>(); } }
        public ISynchronizerService Synchronizer { get { return Get<ISynchronizerService>(); } }
        public IStoreSyncHeads SyncStore { get { return Get<IStoreSyncHeads>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }
        public FakeRequestResponseFactory FakeFactory { get { return Get<FakeRequestResponseFactory>(); } }


        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IRepository Repository { get { return Get<IRepository>(); } }
        public IStoreEvents EventStore { get { return Get<IStoreEvents>(); } }


        public IDispatchCommits Dispatcher { get { return Get<IDispatchCommits>(); } }
        public IAuthUser CurrentUser { get { return Get<IUserService>().CurrentUser; } }
        public FakeHttpClient HttpClient { get { return kernel.Get<IHttpClient>() as FakeHttpClient; } }
        public CompareObjects Comparer { get { return new CompareObjects(); } }

        [Test]
        public void TestPendingSync()
        {

            var PlantId = Guid.NewGuid();
            var Name = "Jore";
            var GardenId = Guid.NewGuid();

            //var ZeroHead = new SyncHead(PlantId, 0);
            //Assert.IsTrue(ZeroHead == new SyncHead(PlantId, 1));


            var UserId = Guid.NewGuid();
            Handler.Handle<Garden, CreateGarden>(new CreateGarden(GardenId));
            Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name, UserId));
            Handler.Handle<Garden, AddPlant>(new AddPlant(GardenId, PlantId, Name));
            Handler.Handle<Plant, MarkPlantPublic>(new MarkPlantPublic(PlantId));

            //Assert.IsTrue(SyncStore.GetSyncHeads().Contains(ZeroHead), SyncStore.GetSyncHeads().Count().ToString());

            //var syncEvents = Synchronizer.UpdatedStreams().Aggregate(0, (acc, stream) => acc + stream.Events.Count());
            Assert.AreEqual(4, Synchronizer.GetPushRequest().Streams.Aggregate(0, (acc, stream) => acc + stream.CommittedEvents.Count()));




            //Assert.IsTrue(Comparer.Compare(syncEvents[0], new PlantCreated(PlantId, Name) { EntityVersion = 1 }));
            //Assert.IsFalse(Comparer.Compare(syncEvents[0], new PlantCreated(Guid.Empty, Name) { EntityVersion = 1 }));
            //Assert.IsTrue(Comparer.Compare(syncEvents[1], new MarkedPlantPublic(PlantId) { EntityVersion = 2 }));
            //Assert.IsFalse(Comparer.Compare(syncEvents[1], new MarkedPlantPublic(Guid.Empty) { EntityVersion = 2 }));



        }

        [Test]
        public async Task TestAsyncSyncDispatch()
        {

            var PlantId = Guid.NewGuid();
            var Name = "Jore";
            var GardenId = Guid.NewGuid();
            var UserId = Guid.NewGuid();

            Handler.Handle<Garden, CreateGarden>(new CreateGarden(GardenId));
            Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name, UserId));
            Handler.Handle<Garden, AddPlant>(new AddPlant(GardenId, PlantId, Name));
            Handler.Handle<Plant, MarkPlantPublic>(new MarkPlantPublic(PlantId));



            var reqq = Synchronizer.GetPushRequest();

            FakeFactory.BuildPushResponse = (ISyncPushRequest request) =>
            {
                return new HttpPushResponse()
                {
                    ClientDatabaseId = request.ClientDatabaseId,
                    StatusCode = 200,
                    StatusDesc = "OK"
                };
            };

            var resp = await Transporter.PushAsync(reqq);

            Console.WriteLine(toJSON(reqq));
            Console.WriteLine(toJSON(resp));

            Assert.AreNotEqual(reqq.ClientDatabaseId, Guid.Empty);
            Assert.AreEqual(reqq.ClientDatabaseId, resp.ClientDatabaseId);


        }


        [Test]
        public async Task TestRebase()
        {
            var PlantId = Guid.NewGuid();
            var Name = "Jore";
            var GardenId = Guid.NewGuid();
            var SynchronizerId = Guid.NewGuid();
            var uCmd = new CreateUser(Guid.NewGuid(), "Alice", "swordfish", "alice@wonderland.net");

            Handler.Handle<User, CreateUser>(uCmd);
            Handler.Handle<Garden, CreateGarden>(new CreateGarden(GardenId));
            Handler.Handle<Garden, AddPlant>(new AddPlant(GardenId, PlantId, Name));
            Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name, uCmd.EntityId));
            Handler.Handle<Synchronizer, CreateSynchronizer>(new CreateSynchronizer(SynchronizerId));

            //var req = RequestFactory.CreatePushRequest(Rebaser.Pending());



            FakeFactory.BuildPushResponse = (ISyncPushRequest request) =>
            {
                return new HttpPushResponse()
                {
                    StatusCode = 200,
                    StatusDesc = "OK",
                    AlreadyExecuted = false
                };
            };


            await SyncHandler.HandleAsync(new Synchronize(SynchronizerId));
            Assert.AreEqual(1, SyncHandler.PushRequests.Count());
            Assert.AreEqual(1, SyncHandler.PushResponses.Count());
            Assert.AreEqual(0, SyncHandler.PullRequests.Count());
            Assert.AreEqual(0, SyncHandler.PullResponses.Count());



            Handler.Handle<Plant, MarkPlantPublic>(new MarkPlantPublic(PlantId));
            var localNote = "LOCAL NOTE";
            var remoteNote = "REMOTE NOTE";

            Handler.Handle<User, Comment>(new Comment(uCmd.EntityId, PlantId, localNote));


            var newPlantId = Guid.NewGuid();
            var newPlantName = "Jore";

            FakeFactory.BuildPushResponse = (ISyncPushRequest request) =>
            {

                return new HttpPushResponse()
                 {
                     StatusCode = 404,
                     StatusDesc = "Needs pulling",
                     AlreadyExecuted = false
                 };
            };

            FakeFactory.BuildPullResponse = (request) =>
            {
                return Tuple.Create<HttpPullResponse, Func<ISyncPushRequest, ISyncPushResponse>>(
                        new HttpPullResponse()
                        {
                            DTOs = new List<EventDTOUnion>()
                            {
                                new EventDTOUnion() {
                                    EventType = DTOType.addComment,
                                    EntityId = uCmd.EntityId,
                                    EntityVersion = 2,
                                    EventId = Guid.NewGuid(),
                                    Note = remoteNote,
                                    ParentId = Guid.NewGuid(),
                                    AncestorId = uCmd.EntityId,
                                    PlantId = PlantId,
                                    PlantAncestorId = uCmd.EntityId
                                },
                                new EventDTOUnion() {
                                    EventType = DTOType.createPlant,
                                    EntityId = newPlantId,
                                    EntityVersion = 1,
                                    EventId = Guid.NewGuid(),
                                    Name = newPlantName,
                                    ParentId = Guid.NewGuid(),
                                    AncestorId = uCmd.EntityId,
                                    ParentAncestorId = uCmd.EntityId
                                }
                            }
                        }
                    ,
                        (ISyncPushRequest reqq) =>
                        {
                            throw new TaskCanceledException("BLAA");
                            return new HttpPushResponse()
                            {
                                StatusCode = 200,
                                StatusDesc = "OK",
                                AlreadyExecuted = false
                            };
                        }
                    );

            };


            try
            {
                await SyncHandler.HandleAsync(new Synchronize(SynchronizerId));

            }
            catch (TaskCanceledException)
            {


            }



            //plantStream = EventStore.OpenStream(PlantId, 0, int.MaxValue);
            //foreach (var e in plantStream.CommittedEvents)
            //{
            //    var ee = ((IEvent)e.Body);
            //    Console.WriteLine(string.Format("{0}, {1}", ee.EntityVersion, ee.ToString()));
            //}


            Assert.AreEqual(2, SyncHandler.PushRequests.Count());
            Assert.AreEqual(1, SyncHandler.PushResponses.Count());
            Assert.AreEqual(1, SyncHandler.PullRequests.Count());
            Assert.AreEqual(1, SyncHandler.PullResponses.Count());

            var req = Synchronizer.GetPushRequest();
            Assert.IsNotNull(req);
            //var reqStreams = req.Streams.ToArray();
            //Assert.AreEqual(2, req.Streams.Count);
            //Assert.AreEqual(1, reqStreams[0].Commits.Length);
            var userSyncStream = req.Streams.Single(x => x.StreamId == uCmd.EntityId);
            var uSE = userSyncStream.Events().ToArray();
            var Comment = uSE[0] as Commented;
            Assert.IsInstanceOf<Commented>(Comment);
            Assert.AreEqual(Comment.EntityVersion, 3);
            Assert.AreEqual(Comment.Note, localNote);


            var userStream = EventStore.OpenStream(uCmd.EntityId, 0, int.MaxValue);
            Assert.AreEqual(3, userStream.CommittedEvents.Count);
            var uE = userStream.Events().ToArray();
            Comment = uE[1] as Commented;
            Assert.IsInstanceOf<Commented>(Comment);
            Assert.AreEqual(Comment.EntityVersion, 2);
            Assert.AreEqual(Comment.Note, localNote);
            Comment = uE[2] as Commented;
            Assert.IsInstanceOf<Commented>(Comment);
            Assert.AreEqual(Comment.EntityVersion, 3);
            Assert.AreEqual(Comment.Note, remoteNote);



            var newPlantStream = EventStore.OpenStream(newPlantId, 0, int.MaxValue);
            Assert.AreEqual(1, newPlantStream.CommittedEvents.Count);
            var CreatedE = (PlantCreated)(newPlantStream.CommittedEvents.Single()).Body;
            Assert.AreEqual(newPlantName, CreatedE.Name);



        }










    }
}
