using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using CommonDomain.Persistence.EventStore;
using EventStore;
using EventStore.Dispatcher;
using EventStore.Logging;
using EventStore.Persistence;
using EventStore.Persistence.SqlPersistence;
using EventStore.Serialization;
using Growthstories.Core;
//using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Messaging;
//using Growthstories.Domain.Entities;
//using Growthstories.Domain.Messaging;
using Growthstories.Domain.Services;
using Growthstories.Sync;
using Growthstories.UI;
using Growthstories.UI.Persistence;


using Ninject;
using Ninject.Modules;
using SQLite;
using System;
using System.IO;
using System.Reflection;
using System.Reactive.Linq;
using System.Linq;
using Growthstories.UI.ViewModel;
//using GalaSoft.MvvmLight.Messaging;
using ReactiveUI;
using EventStore.Persistence.InMemoryPersistence;
using Growthstories.Domain.Entities;
using System.Collections.Generic;
using Growthstories.UI.Services;
using System.Threading.Tasks;


namespace Growthstories.DomainTests
{


    public class SyncEngineTestsSetup : TestModule
    {

        protected override void HttpConfiguration()
        {
            Bind<IHttpClient, ITransportEvents, FakeHttpClient>().To<FakeHttpClient>().InSingletonScope();
            Bind<IEndpoint, FakeEndpoint>().To<FakeEndpoint>().InSingletonScope();
            Bind<IRequestFactory, RequestFactory>().To<RequestFactory>().InSingletonScope();
            Bind<IResponseFactory, ResponseFactory>().To<ResponseFactory>().InSingletonScope();


        }

        protected override void PersistenceConfiguration()
        {
            Bind<IPersistSyncStreams, IPersistStreams>()
                .To<SQLitePersistenceEngine>()
                .InSingletonScope()
                .OnActivation((ctx, eng) =>
                {
                    eng.ReInitialize();
                });


            Bind<IUIPersistence>().To<SQLiteUIPersistence>()
                .InSingletonScope()
                .OnActivation((ctx, eng) =>
                {
                    eng.ReInitialize();
                });

        }

        protected override void UserConfiguration()
        {
            Bind<IUserService>().To<TestUserService>().InSingletonScope();
        }




    }




}


