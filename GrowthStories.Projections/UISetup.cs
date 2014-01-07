using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using CommonDomain.Persistence.EventStore;
using EventStore;
using EventStore.Dispatcher;
using EventStore.Logging;
using EventStore.Persistence;
using EventStore.Serialization;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Messaging;
using Growthstories.Domain.Services;
using Growthstories.Sync;
using Ninject;
using Ninject.Modules;
using System.IO;
using System.Reflection;


namespace Growthstories.UI
{
    public abstract class UISetup : NinjectModule
    {

        protected abstract void HttpConfiguration();
        //{
        //    Bind<IHttpClient>().To<FakeHttpClient>().InSingletonScope();
        //    Bind<IEndpoint>().To<StagingEndpoint>();
        //    Bind<IRequestFactory>().To<HttpRequestFactory>().InSingletonScope();
        //    Bind<IResponseFactory>().To<FakeSyncFactory>().InSingletonScope();
        //    Bind<IHttpRequestFactory>().ToMethod(_ => (FakeSyncFactory)_.Kernel.Get<IResponseFactory>());

        //}

        protected abstract void UserConfiguration();
        //{
        //    Bind<IUserService>().To<FakeUserService>().InSingletonScope();
        //}

        protected abstract void EventFactoryConfiguration();

        protected abstract void LogConfiguration();
        //{
        //    XmlConfigurator.Configure();
        //LogFactory.BuildLogger = type => new LogTo4Net(type);
        //}
        protected abstract void EventStoreConfiguration();
        //Bind<IPersistSyncStreams>()
        //    .To<SQLitePersistenceEngine>()
        //    .InSingletonScope()
        //    .WithConstructorArgument("path", Path.Combine(Directory.GetCurrentDirectory(), "testdb2.sdf"))
        //    .OnActivation((ctx, eng) =>
        //    {
        //        eng.Initialize();
        //        eng.Purge();
        //    });

        //Bind<IPersistStreams>().ToMethod(_ => _.Kernel.Get<IPersistSyncStreams>());

        //#region PipelineHooks
        //Bind<IPipelineHook>().To<OptimisticPipelineHook>().InSingletonScope();
        //Bind<IPipelineHook>().To<DispatchSchedulerPipelineHook>().InSingletonScope();
        //Bind<IScheduleDispatches>().To<SynchronousDispatchScheduler>().InSingletonScope();

        //Bind<IAsyncDispatchCommits>().To<EventDispatcher>().InSingletonScope();
        //Bind<IDispatchCommits>().ToMethod(_ => _.Kernel.Get<IAsyncDispatchCommits>());
        //Bind<IRegisterEventHandlers>().ToMethod(_ => (EventDispatcher)_.Kernel.Get<IDispatchCommits>());
        //Bind<IStoreEvents>().To<OptimisticEventStore>().InSingletonScope();
        //Bind<ICommitEvents>().ToMethod(_ => (OptimisticEventStore)_.Kernel.Get<IStoreEvents>());
        //Bind<ISerialize>().To<JsonSerializer>();
        //Bind<IDetectConflicts>().To<ConflictDetector>().InSingletonScope();
        //Bind<IGSRepository>().To<GSRepository>().InSingletonScope();


        protected abstract void SyncConfiguration();
        //Bind<ITranslateEvents>().To<SyncTranslator>().InSingletonScope();
        //Bind<ITransportEvents>().To<HttpSyncTransporter>().InSingletonScope();

        //Bind<ISynchronizerService>().To<SynchronizerService>().InSingletonScope();
        //Bind<IJsonFactory>().To<JsonFactory>().InSingletonScope();

        protected abstract void DomainConfiguration();
        //Bind<IDispatchCommands>().To<CommandHandler>().InSingletonScope();
        //Bind<IRegisterHandlers>().To<SynchronizerCommandHandler>().InSingletonScope();
        //Bind<IAggregateFactory>().To<AggregateFactory>().InSingletonScope();


        protected abstract void RegisterHandlers(IRegisterEventHandlers registry, IKernel kernel);

        //Bind<IEventHandler<PlantCreated>>().To<PlantProjection>().InSingletonScope();
        //Bind<IAsyncEventHandler<UserSynchronized>>().To<AuthTokenService>().InSingletonScope();

        //registry.Register<PlantCreated>(Kernel.Get<IEventHandler<PlantCreated>>());
        //registry.RegisterAsync<UserSynchronized>(Kernel.Get<IAsyncEventHandler<UserSynchronized>>());

        public override void Load()
        {
            // configure logging
            //
            LogConfiguration();
            HttpConfiguration();
            UserConfiguration();
            EventFactoryConfiguration();
            EventStoreConfiguration();




            RegisterHandlers(Kernel.Get<IRegisterEventHandlers>(), Kernel);

        }


    }


}


