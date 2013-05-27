using System;
using System.Linq;

using NUnit.Framework;
using Growthstories.Domain.Messaging;
using Ninject;
using Growthstories.Core;
using CommonDomain.Persistence;
using Growthstories.Sync;
using Ninject.Parameters;

namespace Growthstories.DomainTests
{
    public class SyncTest
    {
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

        [Test]
        public void TestSyncDispatch()
        {
            var PlantId = Guid.NewGuid();
            var ZeroHead = new SyncHead(PlantId, 0);
            Assert.IsTrue(ZeroHead == new SyncHead(PlantId, 1));

            Handler.Handle(new CreatePlant(PlantId));
            Handler.Handle(new MarkPlantPublic(PlantId));

            Assert.IsTrue(SyncStore.GetSyncHeads().Contains(ZeroHead));
        }

        [Test]
        public void TestSyncDispatch2()
        {
            var PlantId = Guid.NewGuid();
            var ZeroHead = new SyncHead(PlantId, 0);

            Handler.Handle(new CreatePlant(PlantId));
            Handler.Handle(new MarkPlantPrivate(PlantId));

            Assert.IsFalse(SyncStore.GetSyncHeads().Contains(ZeroHead));
        }





    }
}
