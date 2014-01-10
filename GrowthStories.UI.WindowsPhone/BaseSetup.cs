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


namespace Growthstories.UI.WindowsPhone
{
    public abstract class BaseSetup : NinjectModule
    {

        protected virtual void HttpConfiguration()
        {
            Bind<IHttpClient, ITransportEvents, FakeHttpClient>().To<FakeHttpClient>().InSingletonScope();
            Bind<SyncHttpClient>().To<SyncHttpClient>().InSingletonScope();
            Bind<IEndpoint, FakeEndpoint>().To<FakeEndpoint>().InSingletonScope();
            Bind<IRequestFactory, RequestFactory>().To<RequestFactory>().InSingletonScope();
            Bind<IResponseFactory, ResponseFactory>().To<ResponseFactory>().InSingletonScope();
            Bind<FakeHttpRequestFactory>().To<FakeHttpRequestFactory>().InSingletonScope();


        }

        protected abstract void FileSystemConfiguration();



        protected virtual void UserConfiguration()
        {
            Bind<IUserService, AppUserService>().To<AppUserService>().InSingletonScope();
            //Bind<IUserService>().To<FakeUIContext>().InSingletonScope();


        }

        protected virtual void EventFactoryConfiguration()
        {
            Bind<IEventFactory>().To<EventFactory>().InSingletonScope();
        }

        protected virtual void SQLiteConnectionConfiguration()
        {
            SQLiteConnection conn = null;
            Func<SQLiteConnection> del = () =>
            {
                if (conn == null)
                {

                    conn = new SQLiteConnection(Path.Combine(Directory.GetCurrentDirectory(), "testdbb.sqlite"));

                }
                return conn;
            };
            Bind<ISQLiteConnectionFactory>().To<DelegateConnectionFactory>().WithConstructorArgument("f", (object)del);
        }

        protected virtual void PersistenceConfiguration()
        {
            //Bind<IPersistSyncStreams, IPersistStreams>()
            //    .To<SQLitePersistenceEngine>()
            //    .InSingletonScope()
            //    .OnActivation((ctx, eng) =>
            //    {
            //        eng.Initialize();
            //        eng.Purge();
            //    });

            Bind<IPersistSyncStreams, IPersistStreams>()
                .To<SerializingInMemoryPersistenceEngine>()
                .InSingletonScope();


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
            //#if DEBUG

            // LOGGING SERVER is defined in App.xaml.cs for now
            LogFactory.BuildLogger = type => new GSRemoteLog(type);

            //#endif

        }

        public override void Load()
        {

            LogConfiguration();
            HttpConfiguration();
            UserConfiguration();
            EventFactoryConfiguration();
            SQLiteConnectionConfiguration();
            PersistenceConfiguration();
            FileSystemConfiguration();

            Bind<ITranslateEvents>().To<SyncTranslator>().InSingletonScope();




            #region EventStore


            //Bind<IStoreEvents>().To<GSEventStore>();



            #region PipelineHooks
            Bind<IPipelineHook, OptimisticPipelineHook>().To<OptimisticPipelineHook>().InSingletonScope();
            Bind<IPipelineHook>().To<DispatchSchedulerPipelineHook>().InSingletonScope();
            Bind<IScheduleDispatches>().To<SynchronousDispatchScheduler>().InSingletonScope();
            Bind<IMessageBus>().To<MessageBus>().InSingletonScope();

            //Bind<IDispatchCommits>().To<MessageBusDispatcher>().InSingletonScope();

            Bind<IDispatchCommits, MessageBusDispatcher>().To<MessageBusDispatcher>();


            #endregion



            //Bind<GSEventStore>().To<GSEventStore>().InSingletonScope();
            Bind<IStoreEvents, ICommitEvents, GSEventStore>().ToMethod(x =>
            {
                var hooks = x.Kernel.GetAll<IPipelineHook>().ToArray();
                return new GSEventStore(x.Kernel.Get<IPersistStreams>(), hooks);

            }).InSingletonScope();
            Bind<ISerialize>().To<JsonSerializer>();

            #endregion


            Bind<IGSRepository>().To<GSRepository>().InSingletonScope();
            Bind<IDispatchCommands>().To<CommandHandler>().InSingletonScope();

            Bind<ISynchronizerService>().To<SynchronizerService>().InSingletonScope();

            //Bind<IRegisterHandlers>().To<SynchronizerCommandHandler>().InSingletonScope();
            Bind<IAggregateFactory>().To<AggregateFactory>().InSingletonScope();



            Bind<IDetectConflicts>().To<ConflictDetector>().InSingletonScope();
            Bind<IJsonFactory>().To<JsonFactory>().InSingletonScope();




        }



    }




}


