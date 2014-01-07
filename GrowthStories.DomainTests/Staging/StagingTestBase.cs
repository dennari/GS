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



        protected AppViewModel App;
        protected IKernel Kernel { get; set; }
        protected IAuthUser Ctx { get; set; }

        [SetUp]
        public virtual void SetUp()
        {
            if (Kernel != null)
                Kernel.Dispose();
            Kernel = new StandardKernel(new StagingModule());
            App = new StagingAppViewModel(Kernel);
            //Ctx = Get<IUserService>().CurrentUser;

        }


        private ILog Log = new LogToNLog(typeof(StagingTestBase));

        public T Get<T>() { return Kernel.Get<T>(); }
        public IMessageBus Handler { get { return Get<IMessageBus>(); } }


        public ISynchronizerService Synchronizer { get { return Get<ISynchronizerService>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }

        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public ITranslateEvents Translator { get { return Get<ITranslateEvents>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IGSRepository Repository { get { return Get<IGSRepository>(); } }
        public IStoreEvents EventStore { get { return Get<IStoreEvents>(); } }

        public IMessageBus Bus { get { return Get<IMessageBus>(); } }
        public IDispatchCommits Dispatcher { get { return Get<IDispatchCommits>(); } }


        public IUserService UserService { get { return Get<IUserService>(); } }

        public SyncHttpClient HttpClient { get { return (SyncHttpClient)Kernel.Get<IHttpClient>(); } }
        public CompareObjects Comparer { get { return new CompareObjects(); } }


        public static Guid SynchronizerId = Guid.NewGuid();






        public ISyncPushResponse SyncAssertions(ISyncInstance syncResult, bool hasPush = false)
        {
            // if everything goes smoothly, we should have a single pull and a single push
            //Assert.AreEqual(1, syncResult.Pushes.Count);
            //Assert.AreEqual(1, syncResult.Pulls.Count);
            //A//ssert.IsNotNull(syncResult.Pulls[0].Item2);
            Assert.AreEqual(GSStatusCode.OK, syncResult.PullResp.StatusCode);

            if (hasPush || syncResult.PushResp != null)
            {
                Assert.IsNotNull(syncResult.PushResp);
                Assert.AreEqual(GSStatusCode.OK, syncResult.PushResp.StatusCode);

            }


            return syncResult.PushResp;
        }

        public ISyncPushResponse SyncAssertions(IEnumerable<ISyncInstance> syncResults)
        {
            // if everything goes smoothly, we should have a single pull and a single push
            //Assert.AreEqual(1, syncResult.Pushes.Count);
            //Assert.AreEqual(1, syncResult.Pulls.Count);
            //A//ssert.IsNotNull(syncResult.Pulls[0].Item2);
            ISyncPushResponse R = null;
            foreach (var s in syncResults)
                R = SyncAssertions(s);

            return R;
        }




    }
}
