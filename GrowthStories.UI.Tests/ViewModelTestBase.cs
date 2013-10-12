using System;
using System.Linq;

using NUnit.Framework;
using Growthstories.Domain.Messaging;
using Ninject;
using Growthstories.Core;
using CommonDomain.Persistence;
using Growthstories.Sync;
using Ninject.Parameters;
using System.Threading.Tasks;
using System.Collections.Generic;
using Growthstories.Domain.Entities;
using CommonDomain;
using EventStore;

using CommonDomain.Persistence.EventStore;
using EventStore.Dispatcher;
using Growthstories.Domain;
using EventStore.Logging;
using Growthstories.UI;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using Growthstories.DomainTests;

namespace Growthstories.UI.Tests
{
    public class TestAppViewModel : AppViewModel
    {


        public TestAppViewModel(IKernel kernel)
            : base()
        {
            Kernel = kernel;
            Kernel.Bind<IScreen>().ToConstant(this);
            Kernel.Bind<IRoutingState>().ToConstant(this.Router);
            this.Bus = kernel.Get<IMessageBus>();
        }



    }

    public class ViewModelTestBase
    {


        protected AppViewModel App;
        protected IKernel Kernel { get; set; }
        protected IAuthUser Ctx { get; set; }

        [SetUp]
        public virtual void SetUp()
        {
            if (Kernel != null)
                Kernel.Dispose();
            Kernel = new StandardKernel(new TestModule());
            App = new TestAppViewModel(Kernel);
            this.Ctx = App.Context.CurrentUser;

        }
        //private ILog Log = new LogTo4Net(typeof(GardenViewTest));
        public T Get<T>() { return Kernel.Get<T>(); }
        public IMessageBus Bus { get { return Get<IMessageBus>(); } }
        public ISynchronizerService Synchronizer { get { return Get<ISynchronizerService>(); } }
        public IStoreSyncHeads SyncStore { get { return Get<IStoreSyncHeads>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }
        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IGSRepository Repository { get { return Get<IGSRepository>(); } }
        public IStoreEvents EventStore { get { return Get<IStoreEvents>(); } }


    }

}
