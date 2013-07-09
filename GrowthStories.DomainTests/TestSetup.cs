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
using log4net.Config;
using Ninject;
using Ninject.Modules;
using SQLite;
using System;
using System.IO;
using System.Reflection;


namespace Growthstories.DomainTests
{
    public class TestModule : NinjectModule
    {

        protected virtual void HttpConfiguration()
        {
            Bind<IHttpClient>().To<FakeHttpClient>().InSingletonScope();
            Bind<IEndpoint>().To<StagingEndpoint>();
            Bind<IRequestFactory>().To<HttpRequestFactory>().InSingletonScope();
            Bind<IResponseFactory>().To<FakeSyncFactory>().InSingletonScope();
            Bind<IHttpRequestFactory>().ToMethod(_ => (FakeSyncFactory)_.Kernel.Get<IResponseFactory>());

        }

        protected virtual void UserConfiguration()
        {
            Bind<IUserService>().To<FakeUserService>().InSingletonScope();
        }

        protected virtual void EventFactoryConfiguration()
        {
            Bind<IEventFactory>().To<FakeEventFactory>().InSingletonScope();
        }

        public override void Load()
        {
            // configure logging
            XmlConfigurator.Configure();
            HttpConfiguration();
            UserConfiguration();
            EventFactoryConfiguration();

            Bind<ITranslateEvents>().To<SyncTranslator>().InSingletonScope();
            Bind<ITransportEvents>().To<HttpSyncTransporter>().InSingletonScope();


            LogFactory.BuildLogger = type => new LogTo4Net(type);

            #region EventStore

            SQLiteConnection conn = null;
            Func<SQLiteConnection> del = () =>
            {
                if (conn == null)
                    conn = new SQLiteConnection(Path.Combine(Directory.GetCurrentDirectory(), "testdb2.sdf"));
                return conn;
            };
            Bind<ISQLiteConnectionFactory>().To<DelegateConnectionFactory>().WithConstructorArgument("f", (object)del);

            Bind<IPersistSyncStreams>()
                .To<SQLitePersistenceEngine>()
                .InSingletonScope()
                .OnActivation((ctx, eng) =>
                {
                    eng.Initialize();
                    eng.Purge();
                });

            Bind<IPersistStreams>().ToMethod(_ => _.Kernel.Get<IPersistSyncStreams>());

            #region PipelineHooks
            Bind<IPipelineHook>().To<OptimisticPipelineHook>().InSingletonScope();
            Bind<IPipelineHook>().To<DispatchSchedulerPipelineHook>().InSingletonScope();
            Bind<IScheduleDispatches>().To<SynchronousDispatchScheduler>().InSingletonScope();

            Bind<IAsyncDispatchCommits>().To<EventDispatcher>()
                .InSingletonScope()
                .OnActivation((ctx, hndlr) =>
                {
                    //this.RegisterHandlers(hndlr, ctx.Kernel);
                });

            Bind<IDispatchCommits>().ToMethod(_ => _.Kernel.Get<IAsyncDispatchCommits>());
            Bind<IRegisterEventHandlers>().ToMethod(_ => (EventDispatcher)_.Kernel.Get<IDispatchCommits>());

            #endregion

            Bind<IStoreEvents>().To<OptimisticEventStore>().InSingletonScope();
            Bind<ICommitEvents>().ToMethod(_ => (OptimisticEventStore)_.Kernel.Get<IStoreEvents>());
            Bind<ISerialize>().To<JsonSerializer>();

            #endregion


            Bind<IGSRepository>().To<GSRepository>().InSingletonScope();
            Bind<IDispatchCommands>().To<CommandHandler>().InSingletonScope();
            Bind<ISynchronizerService>().To<SynchronizerService>().InSingletonScope();

            Bind<IRegisterHandlers>().To<SynchronizerCommandHandler>().InSingletonScope();


            Bind<IAggregateFactory>().To<AggregateFactory>().InSingletonScope();



            Bind<IDetectConflicts>().To<ConflictDetector>().InSingletonScope();
            Bind<IJsonFactory>().To<JsonFactory>().InSingletonScope();
            Bind<IUIPersistence>().To<SQLiteUIPersistence>()
                .InSingletonScope()
                .OnActivation((ctx, eng) =>
                {
                    eng.Initialize();
                    eng.Purge();
                });


            RegisterHandlers(Kernel.Get<IRegisterEventHandlers>(), Kernel);

        }


        void RegisterHandlers(IRegisterEventHandlers registry, IKernel kernel)
        {

            //Bind<IEventHandler<PlantCreated>>().To<PlantProjection>().InSingletonScope();

            Bind<ActionProjection>().ToSelf().InSingletonScope();
            Bind<PlantProjection>().ToSelf().InSingletonScope();
            Bind<AuthTokenService>().ToSelf().InSingletonScope();

            //Bind<IEventHandler<Commented>>().To<ActionProjection>().InSingletonScope();
            //Bind<IEventHandler<Watered>>().To<ActionProjection>().InSingletonScope();
            //Bind<IEventHandler<Photographed>>().To<ActionProjection>().InSingletonScope();

            Bind<IAsyncEventHandler<UserSynchronized>>().To<AuthTokenService>().InSingletonScope();
            var pproj = Kernel.Get<PlantProjection>();
            registry.Register<PlantCreated>(pproj);
            var aproj = Kernel.Get<ActionProjection>();
            registry.Register<Commented>(aproj);
            registry.Register<Watered>(aproj);
            registry.Register<Photographed>(aproj);
            registry.RegisterAsync<UserSynchronized>(Kernel.Get<AuthTokenService>());
        }

    }

    public class StagingModule : TestModule
    {

        protected override void HttpConfiguration()
        {
            Bind<IHttpClient>().To<SyncHttpClient>().InSingletonScope();
            Bind<IEndpoint>().To<StagingEndpoint>();
            Bind<IRequestFactory>().To<HttpRequestFactory>().InSingletonScope();
            Bind<IResponseFactory>().To<HttpRequestFactory>().InSingletonScope();
            Bind<IHttpRequestFactory>().To<HttpRequestFactory>().InSingletonScope();
        }

        protected override void UserConfiguration()
        {
            Bind<IUserService>().To<SyncUserService>().InSingletonScope();
        }

        protected override void EventFactoryConfiguration()
        {
            Bind<IEventFactory>().To<EventFactory>().InSingletonScope();
        }

    }


}


