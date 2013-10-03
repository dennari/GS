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
using Growthstories.UI;
using System.Text;
using ReactiveUI;
using Growthstories.UI.ViewModel;

namespace Growthstories.DomainTests
{
    public class StagingTestBase
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

        protected AppViewModel App;
        protected IKernel Kernel { get; set; }
        protected IAuthUser Ctx { get; set; }

        [SetUp]
        public virtual void SetUp()
        {
            if (Kernel != null)
                Kernel.Dispose();
            Kernel = new StandardKernel(new StagingModule());
            App = new TestAppViewModel(Kernel);
            Ctx = Get<IUserService>().CurrentUser;

        }


        private ILog Log = new LogToNLog(typeof(StagingTestBase));

        public T Get<T>() { return Kernel.Get<T>(); }
        public IMessageBus Handler { get { return Get<IMessageBus>(); } }


        public ISynchronizerService Synchronizer { get { return Get<ISynchronizerService>(); } }
        public IStoreSyncHeads SyncStore { get { return Get<IStoreSyncHeads>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }

        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public ITranslateEvents Translator { get { return Get<ITranslateEvents>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IGSRepository Repository { get { return Get<IGSRepository>(); } }
        public GSEventStore EventStore { get { return (GSEventStore)Get<IStoreEvents>(); } }

        public IMessageBus Bus { get { return Get<IMessageBus>(); } }
        public IDispatchCommits Dispatcher { get { return Get<IDispatchCommits>(); } }


        public IUserService UserService { get { return Get<IUserService>(); } }

        public SyncHttpClient HttpClient { get { return (SyncHttpClient)Kernel.Get<IHttpClient>(); } }
        public CompareObjects Comparer { get { return new CompareObjects(); } }


        public static Guid SynchronizerId = Guid.NewGuid();




        protected string randomize(string i)
        {
            //var b = new StringBuilder(i);
            //b.Append(Guid.NewGuid().ToString().Substring(0, 4));
            return i + Guid.NewGuid().ToString().Substring(0, 4);
        }






    }
}
