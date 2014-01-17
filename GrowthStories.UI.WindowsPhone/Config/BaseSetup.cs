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
using Ninject;
using Ninject.Modules;
using ReactiveUI;
using SQLite;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;


namespace Growthstories.UI.WindowsPhone
{
    public abstract class BaseSetup : NinjectModule
    {



        public override void Load()
        {

            LogConfiguration();
            HttpConfiguration();
            UserConfiguration();
            EventFactoryConfiguration();
            SQLiteConnectionConfiguration();
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
            Bind<IIAPService>().To<GSIAPMock>().InSingletonScope();

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
            Bind<IScheduleDispatches>().To<SynchronousDispatchScheduler>().InSingletonScope();
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

            //LogFactory.BuildLogger = type => new GSRemoteLog(type);
            //GSLogFactory.BuildLogger = type => new GSRemoteLog(type);

            LogFactory.BuildLogger = type => GSNullLog.Instance;
            GSLogFactory.BuildLogger = type => GSNullLog.Instance;

        }


        protected virtual void HttpConfiguration(string host = "default.lan", int port = 80)
        {
            Bind<IHttpClient, ITransportEvents, SyncHttpClient>().To<SyncHttpClient>().InSingletonScope();

            Bind<IEndpoint>().ToConstructor(ctx => new Endpoint(new Uri(string.Format("http://{0}:{1}", host, port)))).InSingletonScope();

            Bind<IRequestFactory, RequestFactory>().To<RequestFactory>().InSingletonScope();
            Bind<IResponseFactory, ResponseFactory>().To<ResponseFactory>().InSingletonScope();

        }

        protected virtual void FileSystemConfiguration()
        {
            Bind<IPhotoHandler>().To<WP8PhotoHandler>();
        }

        protected virtual void SQLiteConnectionConfiguration(string dbname = "GS.sqlite")
        {
            SQLiteConnection conn = null;
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbname);
            Func<SQLiteConnection> del = () =>
            {
                if (conn == null)
                {
                    conn = new SQLiteConnection(path, true);
                }
                return conn;
            };
            Bind<ISQLiteConnectionFactory>().To<DelegateConnectionFactory>().WithConstructorArgument("f", (object)del);
        }

        protected virtual void PersistenceConfiguration()
        {
            Bind<IPersistSyncStreams, IPersistStreams>().To<SQLitePersistenceEngine>().InSingletonScope();
            Bind<IUIPersistence>().To<SQLiteUIPersistence>().InSingletonScope();
        }




    }




}


