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

namespace Growthstories.DomainTests
{
    public class SyncTest
    {

        [Test]
        public void TestPendingSync()
        {

            var PlantId = Guid.NewGuid();
            var Name = "Jore";
            var GardenId = Guid.NewGuid();

            var ZeroHead = new SyncHead(PlantId, 0);
            Assert.IsTrue(ZeroHead == new SyncHead(PlantId, 1));

            Handler.Handle(new CreateGarden(GardenId));


            Handler.Handle(new AddPlant(GardenId, PlantId, Name));
            Handler.Handle(new MarkPlantPublic(PlantId));
            Handler.Handle(new MarkGardenPublic(GardenId));

            Assert.IsTrue(SyncStore.GetSyncHeads().Contains(ZeroHead));

            var syncEvents = Synchronizer.PendingSynchronization().ToArray();
            Assert.AreEqual(5, syncEvents.Length);
            Assert.IsTrue(Comparer.Compare(syncEvents[0], new PlantCreated(PlantId, Name) { EntityVersion = 1 }));
            Assert.IsFalse(Comparer.Compare(syncEvents[0], new PlantCreated(Guid.Empty, Name) { EntityVersion = 1 }));
            Assert.IsTrue(Comparer.Compare(syncEvents[1], new MarkedPlantPublic(PlantId) { EntityVersion = 2 }));
            Assert.IsFalse(Comparer.Compare(syncEvents[1], new MarkedPlantPublic(Guid.Empty) { EntityVersion = 2 }));



        }

        [Test]
        public async Task TestAsyncSyncDispatch()
        {

            var PlantId = Guid.NewGuid();
            var Name = "Jore";
            var GardenId = Guid.NewGuid();

            Handler.Handle(new CreateGarden(GardenId));
            Handler.Handle(new AddPlant(GardenId, PlantId, Name));
            Handler.Handle(new MarkPlantPublic(PlantId));
            Handler.Handle(new MarkGardenPublic(GardenId));

            HttpClient.CreateResponse = (HttpRequestMessage request) =>
            {
                var jreq = request as JsonRequest;
                var req = (ISyncPushRequest)jreq.Inner;
                return new HttpPushResponse()
                {
                    PushId = req.PushId,
                    ClientDatabaseId = req.ClientDatabaseId,
                    LastExecuted = req.Events.ElementAt(req.Events.Count - 1).guid,
                    StatusCode = 200,
                    StatusDesc = "OK",
                    AlreadyExecuted = false
                };
            };

            var reqq = (HttpPushRequest)Synchronizer.GetPushRequest();

            var resp = (HttpPushResponse)await reqq.ExecuteAsync();

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

            Handler.Handle(new CreateGarden(GardenId));
            Handler.Handle(new AddPlant(GardenId, PlantId, Name));
            //Handler.Handle(new MarkPlantPublic(PlantId));
            Handler.Handle(new MarkGardenPublic(GardenId));

            HttpClient.CreateResponse = (HttpRequestMessage request) =>
            {
                var jreq = request as JsonRequest;
                var req = (ISyncPushRequest)jreq.Inner;
                return new HttpPushResponse()
                {
                    PushId = req.PushId,
                    ClientDatabaseId = req.ClientDatabaseId,
                    LastExecuted = req.Events.ElementAt(req.Events.Count - 1).guid,
                    StatusCode = 200,
                    StatusDesc = "OK",
                    AlreadyExecuted = false
                };
            };


            Assert.AreEqual(1, await Synchronizer.Synchronize());

            HttpClient.CreateResponse = (HttpRequestMessage request) =>
            {
                var jreq = request as JsonRequest;
                var req = jreq.Inner as ISyncPushRequest;
                if (req != null)
                    return new HttpPushResponse()
                    {
                        PushId = req.PushId,
                        ClientDatabaseId = req.ClientDatabaseId,
                        LastExecuted = req.Events.ElementAt(req.Events.Count - 1).guid,
                        StatusCode = 404,
                        StatusDesc = "Not OK",
                        AlreadyExecuted = false
                    };
                else
                {
                    return new HttpPullResponse()
                    {
                        Events = new List<IEventDTO>()
                        {
                            new PlantAddedDTO(new PlantAdded(GardenId,Guid.NewGuid(),"Jaakko")) 
                        }
                    };
                }
            };

            Handler.Handle(new MarkPlantPublic(PlantId));

            Assert.AreEqual(1, await Synchronizer.Synchronize());
            //Assert.Are

            //var resp = (HttpPushResponse)await reqq.ExecuteAsync();



        }




        [Test]
        public void TestSyncDispatch2()
        {
            var PlantId = Guid.NewGuid();
            string Name = "Jore";
            var ZeroHead = new SyncHead(PlantId, 0);

            Handler.Handle(new CreatePlant(PlantId, Name));
            Handler.Handle(new MarkPlantPrivate(PlantId));

            Assert.IsFalse(SyncStore.GetSyncHeads().Contains(ZeroHead));
        }


        IKernel kernel;
        [SetUp]
        public void SetUp()
        {
            kernel = new StandardKernel();
            kernel.WireUp2();
        }

        private ICommandHandler<ICommand> _handler;
        public ICommandHandler<ICommand> Handler
        {
            get
            {
                return _handler == null ? _handler = kernel.Get<ICommandHandler<ICommand>>() : _handler;
            }
        }


        protected JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string toJSON(object o)
        {
            return JsonConvert.SerializeObject(o, SerializerSettings);
        }

        public IRepository Repository
        {
            get
            {
                return kernel.Get<IRepository>();
            }
        }

        public FakeHttpClient HttpClient
        {
            get
            {
                return kernel.Get<IHttpClient>() as FakeHttpClient;
            }
        }

        public IStoreSyncHeads SyncStore
        {
            get
            {
                return kernel.Get<IStoreSyncHeads>();
            }
        }

        CompareObjects comparer;
        public CompareObjects Comparer
        {
            get
            {

                return comparer == null ? comparer = new CompareObjects() : comparer;
            }
        }

        public Synchronizer Synchronizer
        {
            get
            {
                return kernel.Get<Synchronizer>();
            }
        }



    }
}
