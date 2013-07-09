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

        IKernel kernel;
        [SetUp]
        public void SetUp()
        {
            if (kernel != null)
                kernel.Dispose();
            kernel = new StandardKernel(new TestModule());
            Log.Info("-----------------------------------------------------------------------------");
        }
        private ILog Log = new LogTo4Net(typeof(SyncTest));

        public T Get<T>() { return kernel.Get<T>(); }
        public IDispatchCommands Handler { get { return Get<IDispatchCommands>(); } }
        public ISynchronizerService Synchronizer { get { return Get<ISynchronizerService>(); } }
        public IStoreSyncHeads SyncStore { get { return Get<IStoreSyncHeads>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }
        public FakeSyncFactory FakeFactory { get { return Get<IResponseFactory>() as FakeSyncFactory; } }


        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IRepository Repository { get { return Get<IRepository>(); } }
        public IStoreEvents EventStore { get { return Get<IStoreEvents>(); } }


        public IDispatchCommits Dispatcher { get { return Get<IDispatchCommits>(); } }
        public IMemento CurrentUser { get { return Get<IUserService>().CurrentUser; } }
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
            Assert.AreEqual(4, Synchronizer.Pending().Aggregate(0, (acc, stream) => acc + stream.CommittedEvents.Count()));




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



            var reqq = RequestFactory.CreatePushRequest(Synchronizer.Pending());

            FakeFactory.BuildPushResponse = (ISyncPushRequest request) =>
            {
                return new HttpPushResponse()
                {
                    ClientDatabaseId = request.ClientDatabaseId
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
            var CurrentUser = this.CurrentUser;


            Handler.Handle<Garden, CreateGarden>(new CreateGarden(GardenId));
            Handler.Handle<Garden, AddPlant>(new AddPlant(GardenId, PlantId, Name));
            Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name, CurrentUser.Id));
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


            var requests = (IList<ISyncRequest>)await Handler.HandlerHandleAsync<Synchronizer, Synchronize>(new Synchronize(SynchronizerId));

            Assert.AreEqual(1, requests.Count());
            Assert.IsInstanceOf<HttpPushRequest>(requests[0]);
            var events = ((HttpPushRequest)requests[0]).EventsFromStreams().ToArray();
            Assert.IsInstanceOf<GardenCreated>(events[0]);
            Assert.IsInstanceOf<PlantAdded>(events[1]);
            Assert.IsInstanceOf<PlantCreated>(events[2]);



            Handler.Handle<Plant, MarkPlantPublic>(new MarkPlantPublic(PlantId));
            Handler.Handle<Plant, AddPhoto>(new AddPhoto(PlantId, "BLOB"));
            Handler.Handle<Plant, AddComment>(new AddComment(PlantId, "COMMENT"));

            var plantStream = EventStore.OpenStream(PlantId, 0, int.MaxValue);
            foreach (var e in plantStream.CommittedEvents)
            {
                var ee = ((IEvent)e.Body);
                Console.WriteLine(string.Format("{0}, {1}", ee.EntityVersion, ee.ToString()));
            }

            var newPlantId = Guid.NewGuid();
            var newPlantName = "Jore";

            FakeFactory.BuildPushResponse = (ISyncPushRequest request) =>
            {

                Assert.AreEqual(2, request.Streams.Count);
                events = request.EventsFromStreams().ToArray();
                Assert.AreEqual(4, events.Length);

                Assert.IsInstanceOf<MarkedPlantPublic>(events[1]);
                Assert.AreEqual(2, events[1].EntityVersion);

                Assert.IsInstanceOf<PhotoAdded>(events[2]);
                Assert.AreEqual(3, events[2].EntityVersion);

                Assert.IsInstanceOf<CommentAdded>(events[3]);
                Assert.AreEqual(events[3].EntityVersion, 4);


                return new HttpPushResponse()
                {
                    StatusCode = 404,
                    StatusDesc = "Needs pulling",
                    AlreadyExecuted = false
                };
            };

            FakeFactory.BuildPullResponse = (request) =>
            {
                return Tuple.Create<ISyncPullResponse, Func<ISyncPushRequest, ISyncPushResponse>>(
                        new HttpPullResponse()
                        {
                            DTOs = new List<EventDTOUnion>()
                            {
                                new EventDTOUnion() {
                                    EventType = DTOType.addComment,
                                    EntityId = PlantId,
                                    EntityVersion = 2,
                                    EventId = Guid.NewGuid(),
                                    Note = "REMOTE COMMENT",
                                    ParentId = Guid.NewGuid(),
                                    AncestorId = CurrentUser.Id,
                                    ParentAncestorId = CurrentUser.Id
                                },
                                new EventDTOUnion() {
                                    EventType = DTOType.createPlant,
                                    EntityId = newPlantId,
                                    EntityVersion = 1,
                                    EventId = Guid.NewGuid(),
                                    Name = newPlantName,
                                    ParentId = Guid.NewGuid(),
                                    AncestorId = CurrentUser.Id,
                                    ParentAncestorId = CurrentUser.Id
                                }
                            }
                        }
                    ,
                        (ISyncPushRequest reqq) =>
                        new HttpPushResponse()
                        {
                            StatusCode = 200,
                            StatusDesc = "OK",
                            AlreadyExecuted = false
                        }
                    );

            };




            requests = (IList<ISyncRequest>)await Handler.HandlerHandleAsync<Synchronizer, Synchronize>(new Synchronize(SynchronizerId));


            plantStream = EventStore.OpenStream(PlantId, 0, int.MaxValue);
            foreach (var e in plantStream.CommittedEvents)
            {
                var ee = ((IEvent)e.Body);
                Console.WriteLine(string.Format("{0}, {1}", ee.EntityVersion, ee.ToString()));
            }


            Assert.AreEqual(3, requests.Count());



            Assert.IsInstanceOf<HttpPushRequest>(requests[2]);
            events = ((HttpPushRequest)requests[2]).EventsFromStreams().ToArray();

            Assert.IsInstanceOf<MarkedPlantPublic>(events[1]);
            Assert.AreEqual(events[1].EntityVersion, 3);

            Assert.IsInstanceOf<PhotoAdded>(events[2]);
            Assert.AreEqual(events[2].EntityVersion, 4);

            Assert.IsInstanceOf<CommentAdded>(events[3]);
            Assert.AreEqual(events[3].EntityVersion, 5);

            var newPlantStream = EventStore.OpenStream(newPlantId, 0, int.MaxValue);
            Assert.AreEqual(1, newPlantStream.CommittedEvents.Count);
            var CreatedE = (PlantCreated)(newPlantStream.CommittedEvents.Single()).Body;
            Assert.AreEqual(newPlantName, CreatedE.Name);



        }










    }
}
