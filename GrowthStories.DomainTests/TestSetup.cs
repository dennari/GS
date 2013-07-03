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
using Growthstories.Projections;
using log4net.Config;
using Ninject;
using Ninject.Modules;
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

            Bind<IPersistSyncStreams>()
                .To<SQLitePersistenceEngine>()
                .InSingletonScope()
                .WithConstructorArgument("path", Path.Combine(Directory.GetCurrentDirectory(), "testdb2.sdf"))
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

            Bind<IAsyncDispatchCommits>().To<EventDispatcher>().InSingletonScope();
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

            RegisterHandlers(Kernel.Get<IRegisterEventHandlers>(), Kernel);

        }

        void RegisterHandlers(IRegisterEventHandlers registry, IKernel kernel)
        {
            Bind<IEventHandler<PlantCreated>>().To<PlantProjection>().InSingletonScope();
            Bind<IAsyncEventHandler<UserSynchronized>>().To<AuthTokenService>().InSingletonScope();

            registry.Register<PlantCreated>(Kernel.Get<IEventHandler<PlantCreated>>());
            registry.RegisterAsync<UserSynchronized>(Kernel.Get<IAsyncEventHandler<UserSynchronized>>());
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


