using System;
using System.Linq;
using CommonDomain;
using CommonDomain.Core;
using EventStore;
using EventStore.Dispatcher;
using EventStore.Logging;
using EventStore.Persistence;
using EventStore.Persistence.SqlPersistence;
using EventStore.Serialization;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Services;
using Growthstories.Sync;
using Growthstories.UI.Persistence;
using Growthstories.UI.Services;
using Growthstories.UI.ViewModel;
using Ninject;
using Ninject.Modules;
using ReactiveUI;
using SQLite;
using GrowthStories.Core;
using Growthstories.UI.WindowsPhone.ViewModels;
//using Windows.Storage;


namespace Growthstories.Configuration
{
    public abstract class BaseSetup : NinjectModule
    {

        protected IMutableDependencyResolver RxUIResolver;


        public override void Load()
        {

            RxUIResolver = RxApp.MutableResolver;

            LogConfiguration();
            RxUIConfiguration();

            HttpConfiguration();
            UserConfiguration();
            EventFactoryConfiguration();
            //SQLiteConnectionConfiguration();
            PersistenceConfiguration();
            FileSystemConfiguration();
            EventStoreConfiguration();
            AppConfiguration();

            Bind<ITranslateEvents>().To<SyncTranslator>().InSingletonScope();
            Bind<IMessageBus>().To<MessageBus>().InSingletonScope();
            Bind<IGSRepository>().To<GSRepository>().InSingletonScope();
            Bind<IDispatchCommands>().To<CommandHandler>().InSingletonScope();
            Bind<IAggregateFactory>().To<AggregateFactory>().InSingletonScope();
            Bind<IDetectConflicts>().To<ConflictDetector>().InSingletonScope();
            Bind<IJsonFactory>().To<JsonFactory>().InSingletonScope();

            //Bind<ISynchronizerService>().To<SynchronizerService>().InSingletonScope();
            //Bind<IRegisterHandlers>().To<SynchronizerCommandHandler>().InSingletonScope();


        }

        protected virtual void AppConfiguration()
        {
            Bind<IIAPService>().To<NullIIAP>().InSingletonScope();
            Bind<IScheduleService>().To<GardenScheduler>()
                .InSingletonScope()
                .WithConstructorArgument("updateInterval", new TimeSpan(0, 0, 10));

            Bind<ISynchronizer>().To<NonAutoSyncingSynchronizer>();

        }

        protected virtual void RxUIConfiguration()
        {
            Bind<IMutableDependencyResolver>().ToConstant(this.RxUIResolver);
            Bind<IRoutingState>().To<RoutingState>().InSingletonScope();
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(IScreen)), typeof(IScreen));
            RxUIResolver.RegisterLazySingleton(() => KernelInstance.GetService(typeof(IRoutingState)), typeof(IRoutingState));
            RxUIResolver.RegisterLazySingleton(() => GSViewLocator.Instance, typeof(IViewLocator));

        }

        protected virtual void UserConfiguration()
        {
            Bind<IUserService, AppUserService>().To<AppUserService>().InSingletonScope();

        }

        protected virtual void EventFactoryConfiguration()
        {
            Bind<IEventFactory>().To<EventFactory>().InSingletonScope();
        }

        protected virtual void EventStoreConfiguration()
        {
            Bind<IPipelineHook, OptimisticPipelineHook>().To<OptimisticPipelineHook>().InSingletonScope();
            Bind<IPipelineHook>().To<DispatchSchedulerPipelineHook>().InSingletonScope();
            Bind<IScheduleDispatches>().To<GSDispatchScheduler>().InSingletonScope();
            Bind<IDispatchCommits, MessageBusDispatcher>().To<MessageBusDispatcher>();
            Bind<IStoreEvents, ICommitEvents, GSEventStore>().ToMethod(x =>
            {
                var hooks = x.Kernel.GetAll<IPipelineHook>().ToArray();
                return new GSEventStore(x.Kernel.Get<IPersistStreams>(), hooks);

            }).InSingletonScope();
            Bind<ISerialize>().To<JsonSerializer>();

        }


        protected virtual void LogConfiguration()
        {

            RxUIResolver.Register(() => GSNullLog.Instance, typeof(ILogger));
            LogFactory.BuildLogger = type => GSNullLog.Instance;
            GSLogFactory.BuildLogger = type => GSNullLog.Instance;

        }


        protected void HttpConfiguration()
        {
            Bind<IHttpClient, ITransportEvents, SyncHttpClient>().To<SyncHttpClient>().InSingletonScope();

            Bind<IEndpoint>().ToConstructor(ctx => new Endpoint(BaseUri())).InSingletonScope();

            Bind<IRequestFactory, RequestFactory>().To<RequestFactory>().InSingletonScope();
            Bind<IResponseFactory, ResponseFactory>().To<ResponseFactory>().InSingletonScope();
        }


        protected virtual Uri BaseUri()
        {
            return new Uri("https://gs-prod.appspot.com");
        }


        protected virtual void FileSystemConfiguration()
        {
            Bind<IPhotoHandler>().To<NullPhotoHandler>();
        }


        protected virtual void PersistenceConfiguration()
        {
            Bind<IPersistSyncStreams, IPersistStreams>().ToConstructor((arg) => new SQLitePersistenceEngine(new DelegateConnectionFactory(SQLConnection), KernelInstance.Get<ISerialize>())).InSingletonScope();
            Bind<IUIPersistence>().ToConstructor((arg) => new SQLiteUIPersistence(new DelegateConnectionFactory(UIConnection), KernelInstance.Get<ISerialize>())).InSingletonScope();
        }

        protected virtual string UIPersistenceDBName()
        {
            return "GSUI.sqlite";
        }

        protected virtual string SQLPersistenceDBName()
        {
            return "GS.sqlite";
        }

        private SQLiteConnection _UIConnection;
        private SQLiteConnection UIConnection()
        {
            if (_UIConnection == null)
            {
                _UIConnection = new SQLiteConnection(UIPersistenceDBName(), SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex, true);
            }
            return _UIConnection;
        }

        private SQLiteConnection _SQLConnection;
        private SQLiteConnection SQLConnection()
        {
            if (_SQLConnection == null)
            {
                _SQLConnection = new SQLiteConnection(SQLPersistenceDBName(), SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex, true);
            }
            return _SQLConnection;
        }


    }




}


