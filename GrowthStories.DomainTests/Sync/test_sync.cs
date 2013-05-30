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

namespace Growthstories.DomainTests
{
    public class SyncTest
    {

        [Test]
        public void TestSyncDispatch()
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

            var req = Synchronizer.GetPushRequest();
            var resp = req.Execute();

            var reqE = req.cmds.ToArray();
            var respE = resp.Events.ToArray();

            Assert.AreEqual(toJSON(reqE[0]), toJSON(respE[0]));
            Assert.AreEqual(toJSON(reqE[1]), toJSON(respE[1]));


            //Console.WriteLine(toJSON(req));
            //Console.WriteLine(toJSON(resp.Events.ToArray()));


            SyncStore.Purge();
            syncEvents = Synchronizer.PendingSynchronization().ToArray();
            Assert.AreEqual(0, syncEvents.Length);

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
            TypeNameHandling = TypeNameHandling.None
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
