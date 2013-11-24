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
using EventStore.Persistence;

namespace Growthstories.DomainTests
{
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
            Kernel = new StandardKernel(new SyncEngineTestsSetup());
            App = new TestAppViewModel(Kernel);

            var u = App.User;
            Assert.IsNotNull(u);
            Assert.IsNotNull(u.Username);
            Assert.IsNull(App.Model.State.User);
            Handler = Get<IDispatchCommands>();
            Handler.Handle(new CreateUser(u.Id, u.Username, u.Password, u.Email));
            Handler.Handle(new AssignAppUser(u.Id, u.Username, u.Password, u.Email));
            Handler.Handle(new CreateGarden(u.GardenId, u.Id));


            //Ctx = Get<IUserService>().CurrentUser;
            Assert.IsNotNull(App.Model.State.User);


        }


        //private ILog Log = new LogToNLog(typeof(StagingTestBase));

        public T Get<T>() { return Kernel.Get<T>(); }
        public IGSRepository Repository { get { return Get<IGSRepository>(); } }
        public IMessageBus Bus { get { return App.Bus; } }
        public IAuthUser U { get { return App.User; } }
        public IDispatchCommands Handler { get; set; }

        protected IUIPersistence _UIPersistence;
        protected IUIPersistence UIPersistence
        {
            get { return _UIPersistence ?? (_UIPersistence = Get<IUIPersistence>()); }
        }

        protected IPersistSyncStreams _Persistence;
        protected IPersistSyncStreams Persistence
        {
            get { return _Persistence ?? (_Persistence = Get<IPersistSyncStreams>()); }
        }




    }
}
