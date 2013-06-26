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
//using Growthstories.Domain.Entities;
//using Growthstories.Domain.Messaging;
using Growthstories.Domain.Services;
using Growthstories.Sync;
using log4net.Config;
using Ninject;
using Ninject.Modules;
using System.IO;


namespace Growthstories.DomainTests
{
    public class TestModule : NinjectModule
    {
        public override void Load()
        {
            // configure logging
            XmlConfigurator.Configure();

            Bind<IStoreSyncHeads>().To<SynchronizerInMemoryPersistence>().InSingletonScope();

            Bind<ITranslateEvents>().To<SyncTranslator>().InSingletonScope();
            Bind<ITransportEvents>().To<HttpSyncTransporter>().InSingletonScope();
            //Bind<ICommunicator>().To<HttpSyncTransporter>().InSingletonScope();
            Bind<IHttpClient>().To<FakeHttpClient>().InSingletonScope();

            //var persistence = new SQLitePersistenceEngine(this.TransformConnectionString(this.GetConnectionString()), this.serializer);

            LogFactory.BuildLogger = type => new LogTo4Net(type);

            Bind<IPersistDeleteStreams>()
                .To<SQLitePersistenceEngine>()
                .InSingletonScope()
                .WithConstructorArgument("path", Path.Combine(Directory.GetCurrentDirectory(), "testdb.sdf"))
                .OnActivation((ctx, eng) =>
                {
                    eng.Purge();
                    eng.Initialize();
                });

            Bind<IPersistStreams>().ToMethod(_ => _.Kernel.Get<IPersistDeleteStreams>());


            Bind<IPipelineHook>().To<OptimisticPipelineHook>().InSingletonScope();
            Bind<IPipelineHook>().To<DispatchSchedulerPipelineHook>().InSingletonScope();
            Bind<IStoreEvents>().To<OptimisticEventStore>().InSingletonScope();
            Bind<ICommitEvents>().ToMethod(_ => (OptimisticEventStore)_.Kernel.Get<IStoreEvents>());
            Bind<IDispatchCommits>().To<SyncDispatcher>().InSingletonScope();
            Bind<IScheduleDispatches>().To<SynchronousDispatchScheduler>().InSingletonScope();
            Bind<IRebaseEvents>().To<SyncRebaser>().InSingletonScope();

            //Bind<IStoreEvents>().ToMethod(_ =>
            //{
            //    return Wireup.Init()
            //        .LogTo()
            //        .UsingSynchronousDispatchScheduler()
            //        .DispatchTo(_.Kernel.Get<IDispatchCommits>())
            //        .Build();

            //}).InSingletonScope();

            Bind<ISerialize>().To<JsonSerializer>();
            Bind<IGSRepository>().To<GSRepository>().InSingletonScope();
            Bind<IDispatchCommands>().To<CommandHandler>().InSingletonScope();
            Bind<IAggregateFactory>().To<AggregateFactory>().InSingletonScope();
            Bind<IEventFactory>().To<FakeEventFactory>().InSingletonScope();

            Bind<IDetectConflicts>().To<ConflictDetector>().InSingletonScope();

            Bind<IAncestorFactory>().To<FakeAncestorFactory>().InSingletonScope();
            Bind<IRequestFactory>().To<HttpRequestFactory>().InSingletonScope();
            Bind<IResponseFactory>().To<HttpRequestFactory>().InSingletonScope();
            Bind<IJsonFactory>().To<JsonFactory>().InSingletonScope();

        }

    }


}


