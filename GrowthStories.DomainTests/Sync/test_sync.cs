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
        public Synchronizer Synchronizer { get { return Get<Synchronizer>(); } }
        public IStoreSyncHeads SyncStore { get { return Get<IStoreSyncHeads>(); } }
        public IRebaseEvents Rebaser { get { return Get<IRebaseEvents>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }

        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IRepository Repository { get { return Get<IRepository>(); } }
        public IDispatchCommits Dispatcher { get { return Get<IDispatchCommits>(); } }
        public IMemento CurrentUser { get { return Get<IAncestorFactory>().GetAncestor(); } }
        public FakeHttpClient HttpClient { get { return kernel.Get<IHttpClient>() as FakeHttpClient; } }
        public CompareObjects Comparer { get { return new CompareObjects(); } }

        [Test]
        public void TestPendingSync()
        {

            var PlantId = Guid.NewGuid();
            var Name = "Jore";
            var GardenId = Guid.NewGuid();

            var ZeroHead = new SyncHead(PlantId, 0);
            Assert.IsTrue(ZeroHead == new SyncHead(PlantId, 1));


            Handler.Handle<Garden, CreateGarden>(new CreateGarden(GardenId));
            Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name));
            Handler.Handle<Garden, AddPlant>(new AddPlant(GardenId, PlantId, Name));
            Handler.Handle<Plant, MarkPlantPublic>(new MarkPlantPublic(PlantId));

            Assert.IsTrue(SyncStore.GetSyncHeads().Contains(ZeroHead), SyncStore.GetSyncHeads().Count().ToString());

            //var syncEvents = Synchronizer.UpdatedStreams().Aggregate(0, (acc, stream) => acc + stream.Events.Count());
            Assert.AreEqual(4, Rebaser.Pending().Aggregate(0, (acc, stream) => acc + stream.Events.Count()));




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

            Handler.Handle<Garden, CreateGarden>(new CreateGarden(GardenId));
            Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name));
            Handler.Handle<Garden, AddPlant>(new AddPlant(GardenId, PlantId, Name));
            Handler.Handle<Plant, MarkPlantPublic>(new MarkPlantPublic(PlantId));



            var reqq = RequestFactory.CreatePushRequest(Rebaser.Pending());

            HttpClient.CreateResponse = (HttpRequestMessage request, int num) =>
            {
                return new HttpPushResponse()
                {
                    PushId = reqq.PushId,
                    ClientDatabaseId = reqq.ClientDatabaseId
                };
            };

            var resp = await Transporter.PushAsync(reqq);

            Console.WriteLine(toJSON(reqq));
            Console.WriteLine(toJSON(resp));

            Assert.AreNotEqual(reqq.PushId, Guid.Empty);
            Assert.AreNotEqual(reqq.ClientDatabaseId, Guid.Empty);
            Assert.AreEqual(reqq.PushId, resp.PushId);
            Assert.AreEqual(reqq.ClientDatabaseId, resp.ClientDatabaseId);


        }


        [Test]
        public async Task TestRebase()
        {
            var PlantId = Guid.NewGuid();
            var Name = "Jore";
            var GardenId = Guid.NewGuid();
            var CurrentUser = this.CurrentUser;


            HttpClient.CreateResponse = (HttpRequestMessage request, int num) =>
            {
                if (num == 1) // this the initial push, to sync the created plant and garden
                {
                    return new HttpPushResponse()
                    {
                        StatusCode = 200,
                        StatusDesc = "OK",
                        AlreadyExecuted = false
                    };
                }

                if (num == 2) // this the failing push
                {
                    return new HttpPushResponse()
                    {
                        StatusCode = 404,
                        StatusDesc = "Needs pulling",
                        AlreadyExecuted = false
                    };
                }

                if (num == 3)// this is the pull
                {
                    return new HttpPullResponse()
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

                            }
                        }
                    };
                }

                // first push after pull
                return new HttpPushResponse()
                {
                    StatusCode = 200,
                    StatusDesc = "OK",
                    AlreadyExecuted = false
                };

            };


            Handler.Handle<Garden, CreateGarden>(new CreateGarden(GardenId));
            Handler.Handle<Plant, CreatePlant>(new CreatePlant(PlantId, Name));
            Handler.Handle<Garden, AddPlant>(new AddPlant(GardenId, PlantId, Name));
            var req = RequestFactory.CreatePushRequest(Rebaser.Pending());
            Assert.AreEqual(1, await Synchronizer.Synchronize());

            Handler.Handle<Plant, MarkPlantPublic>(new MarkPlantPublic(PlantId));
            Handler.Handle<Plant, AddPhoto>(new AddPhoto(PlantId, "BLOB"));
            Handler.Handle<Plant, AddComment>(new AddComment(PlantId, "COMMENT"));
            req = RequestFactory.CreatePushRequest(Rebaser.Pending());
            //Handler.Handle(new MarkGardenPublic(GardenId));


            Assert.AreEqual(2, await Synchronizer.Synchronize());


        }










    }
}
