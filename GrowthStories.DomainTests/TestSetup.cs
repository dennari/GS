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


            Bind<ITranslateEvents>().To<SyncTranslator>().InSingletonScope();
            Bind<ITransportEvents>().To<HttpSyncTransporter>().InSingletonScope();
            Bind<IHttpClient>().To<FakeHttpClient>().InSingletonScope();


            LogFactory.BuildLogger = type => new LogTo4Net(type);

            Bind<IPersistSyncStreams>()
                .To<SQLitePersistenceEngine>()
                .InSingletonScope()
                .WithConstructorArgument("path", Path.Combine(Directory.GetCurrentDirectory(), "testdb.sdf"))
                .OnActivation((ctx, eng) =>
                {
                    eng.Purge();
                    eng.Initialize();
                });

            Bind<IPersistStreams>().ToMethod(_ => _.Kernel.Get<IPersistSyncStreams>());


            Bind<IPipelineHook>().To<OptimisticPipelineHook>().InSingletonScope();
            Bind<IStoreEvents>().To<OptimisticEventStore>().InSingletonScope();
            Bind<ICommitEvents>().ToMethod(_ => (OptimisticEventStore)_.Kernel.Get<IStoreEvents>());


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


