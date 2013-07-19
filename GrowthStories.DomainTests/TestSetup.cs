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

#if !WINDOWS_PHONE
using log4net.Config;
#endif

using Ninject;
using Ninject.Modules;
using SQLite;
using System;
using System.IO;
using System.Reflection;
using Growthstories.UI.ViewModel;
using GalaSoft.MvvmLight.Messaging;


namespace Growthstories.DomainTests
{
    public class TestModule : NinjectModule
    {

        protected virtual void HttpConfiguration()
        {
            Bind<IHttpClient, FakeHttpClient>().To<FakeHttpClient>().InSingletonScope();
            Bind<IEndpoint, FakeEndpoint>().To<FakeEndpoint>().InSingletonScope();
            Bind<IRequestFactory, IResponseFactory, FakeRequestResponseFactory>().To<FakeRequestResponseFactory>().InSingletonScope();
            Bind<IHttpResponseFactory, IHttpRequestFactory, FakeHttpRequestResponseFactory>().To<FakeHttpRequestResponseFactory>().InSingletonScope();

        }

        protected virtual void UIConfiguration()
        {
            Bind<IMessenger>().To<Messenger>().InSingletonScope();
            Bind<INavigationService, FakeNavigationService>().To<FakeNavigationService>().InSingletonScope();
            Bind<GardenViewModel>().ToSelf().InSingletonScope();
            Bind<PlantViewModel>().ToSelf().InSingletonScope();
        }


        protected virtual void UserConfiguration()
        {
            Bind<IUserService>().To<FakeUserService>().InSingletonScope();
            //Bind<IUserService>().To<FakeUIContext>().InSingletonScope();


        }

        protected virtual void EventFactoryConfiguration()
        {
            Bind<IEventFactory>().To<FakeEventFactory>().InSingletonScope();
        }

        protected virtual void SQLiteConnectionConfiguration()
        {
            SQLiteConnection conn = null;
            Func<SQLiteConnection> del = () =>
            {
                if (conn == null)
                {

                    conn = new SQLiteConnection(Path.Combine(Directory.GetCurrentDirectory(), "testdb2.sdf"));

                }
                return conn;
            };
            Bind<ISQLiteConnectionFactory>().To<DelegateConnectionFactory>().WithConstructorArgument("f", (object)del);
        }

        protected virtual void PersistenceConfiguration()
        {
            Bind<IPersistSyncStreams, IPersistStreams>()
                .To<SQLitePersistenceEngine>()
                .InSingletonScope()
                .OnActivation((ctx, eng) =>
                {
                    eng.Initialize();
                    eng.Purge();
                });

            Bind<IUIPersistence>().To<SQLiteUIPersistence>()
                .InSingletonScope()
                .OnActivation((ctx, eng) =>
                {
                    eng.Initialize();
                    eng.Purge();
                });

        }



        protected virtual void LogConfiguration()
        {
            // configure logging
#if !WINDOWS_PHONE
            XmlConfigurator.Configure();
            LogFactory.BuildLogger = type => new GSLog(type);
#endif

        }

        public override void Load()
        {

            LogConfiguration();
            HttpConfiguration();
            UserConfiguration();
            EventFactoryConfiguration();
            SQLiteConnectionConfiguration();
            PersistenceConfiguration();
            UIConfiguration();

            Bind<ITranslateEvents>().To<SyncTranslator>().InSingletonScope();
            Bind<ITransportEvents>().To<HttpSyncTransporter>().InSingletonScope();




            #region EventStore






            #region PipelineHooks
            Bind<IPipelineHook>().To<OptimisticPipelineHook>().InSingletonScope();
            Bind<IPipelineHook>().To<DispatchSchedulerPipelineHook>().InSingletonScope();
            Bind<IScheduleDispatches>().To<SynchronousDispatchScheduler>().InSingletonScope();
            Bind<IDispatchCommits, IAsyncDispatchCommits, IRegisterEventHandlers>().To<EventDispatcher>().InSingletonScope();

            #endregion



            Bind<IStoreEvents, ICommitEvents>().To<GSEventStore>().InSingletonScope();
            Bind<ISerialize>().To<JsonSerializer>();

            #endregion


            Bind<IGSRepository>().To<GSRepository>().InSingletonScope();
            Bind<IDispatchCommands>().To<CommandHandler>().InSingletonScope();
            Bind<ISynchronizerService>().To<SynchronizerService>().InSingletonScope();
            Bind<IConstructSyncEventStreams>().To<SyncEventStreamFactory>().InSingletonScope();

            //Bind<IRegisterHandlers>().To<SynchronizerCommandHandler>().InSingletonScope();
            Bind<IAggregateFactory>().To<AggregateFactory>().InSingletonScope();



            Bind<IDetectConflicts>().To<ConflictDetector>().InSingletonScope();
            Bind<IJsonFactory>().To<JsonFactory>().InSingletonScope();


            Bind<SynchronizerCommandHandler>().ToSelf().InSingletonScope();
            Bind<ActionProjection>().ToSelf().InSingletonScope();
            Bind<PlantProjection>().ToSelf().InSingletonScope();
            Bind<IAuthTokenService>().To<AuthTokenService>().InSingletonScope();

            RegisterHandlers(Kernel.Get<IRegisterEventHandlers>(), Kernel);

        }




        void RegisterHandlers(IRegisterEventHandlers registry, IKernel kernel)
        {
            // Bind<IAsyncEventHandler<UserSynchronized>>().To<AuthTokenService>().InSingletonScope();
            var pproj = Kernel.Get<PlantProjection>();
            registry.Register<PlantCreated>(pproj);
            var aproj = Kernel.Get<ActionProjection>();
            registry.Register<Commented>(aproj);
            registry.Register<Watered>(aproj);
            registry.Register<Photographed>(aproj);
            registry.Register<Fertilized>(aproj);
            //registry.RegisterAsync<UserSynchronized>(Kernel.Get<AuthTokenService>());


            var PVM = kernel.Get<PlantViewModel>();
            kernel.Get<IMessenger>().Register<ShowPlantView>(PVM, PVM.Handle);

        }

    }




}


