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

namespace Growthstories.DomainTests
{
    public class StagingTestBase
    {

        public IKernel kernel;
        [SetUp]
        public virtual void SetUp()
        {
            if (kernel != null)
                kernel.Dispose();
            kernel = new StandardKernel(new StagingModule());
            this.Ctx = Get<IUserService>().CurrentUser;
            Handler.Handle(new CreateSynchronizer(SynchronizerId));

        }
        private ILog Log = new LogTo4Net(typeof(StagingTestBase));

        public T Get<T>() { return kernel.Get<T>(); }
        public IMessageBus Handler { get { return Get<IMessageBus>(); } }
        public IAsyncDispatchCommits AsyncDispatcher { get { return Get<IAsyncDispatchCommits>(); } }

        public SynchronizerCommandHandler SyncHandler { get { return Get<SynchronizerCommandHandler>(); } }

        public ISynchronizerService Synchronizer { get { return Get<ISynchronizerService>(); } }
        public IStoreSyncHeads SyncStore { get { return Get<IStoreSyncHeads>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }

        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IGSRepository Repository { get { return Get<IGSRepository>(); } }
        public IStoreEvents EventStore { get { return Get<IStoreEvents>(); } }


        public IDispatchCommits Dispatcher { get { return Get<IDispatchCommits>(); } }


        public IUserService UserService { get { return Get<IUserService>(); } }

        public IHttpClient HttpClient { get { return kernel.Get<IHttpClient>(); } }
        public CompareObjects Comparer { get { return new CompareObjects(); } }


        public static Guid SynchronizerId = Guid.NewGuid();
        public IAuthUser Ctx;



        protected async Task Sync()
        {

            try
            {
                await SyncHandler.HandleAsync(new Synchronize(SynchronizerId));

            }
            catch (TaskCanceledException)
            {


            }
        }


        protected string randomize(string i)
        {
            //var b = new StringBuilder(i);
            //b.Append(Guid.NewGuid().ToString().Substring(0, 4));
            return i + Guid.NewGuid().ToString().Substring(0, 4);
        }






    }
}
