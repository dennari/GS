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
using Growthstories.UI.ViewModel;
//using GalaSoft.MvvmLight.Messaging;
using ReactiveUI;
using EventStore.Persistence.InMemoryPersistence;
using Growthstories.Domain.Entities;
using System.Collections.Generic;
using Growthstories.UI.Services;


namespace Growthstories.DomainTests
{
    public class TestModule : NinjectModule
    {

        protected virtual void HttpConfiguration()
        {
            Bind<IHttpClient, ITransportEvents, FakeHttpClient>().To<FakeHttpClient>().InSingletonScope();
            Bind<IEndpoint, FakeEndpoint>().To<FakeEndpoint>().InSingletonScope();
            Bind<IRequestFactory, IResponseFactory, FakeRequestResponseFactory>().To<FakeRequestResponseFactory>().InSingletonScope();
            Bind<FakeHttpRequestFactory>().To<FakeHttpRequestFactory>().InSingletonScope();


        }

        protected virtual void UIConfiguration()
        {
            //Bind<INavigationService, FakeNavigationService>().To<FakeNavigationService>().InSingletonScope();
            //Bind<GardenViewModel>().ToSelf().InSingletonScope();
            //Bind<PlantViewModel>().ToSelf().InSingletonScope();
        }


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
#if !WINDOWS_PHONE
            //XmlConfigurator.Configure();
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




            #region EventStore






            #region PipelineHooks
            Bind<IPipelineHook, OptimisticPipelineHook>().To<OptimisticPipelineHook>().InSingletonScope();
            Bind<IPipelineHook>().To<DispatchSchedulerPipelineHook>().InSingletonScope();
            Bind<IScheduleDispatches>().To<SynchronousDispatchScheduler>().InSingletonScope();
            Bind<IMessageBus>().To<MessageBus>().InSingletonScope();

            //Bind<IDispatchCommits>().To<MessageBusDispatcher>().InSingletonScope();

            Bind<IDispatchCommits>().ToConstructor(p => new DelegateMessageDispatcher(this.DoubleDispatch)).InSingletonScope();
            Bind<MessageBusDispatcher>().ToSelf().InSingletonScope();
            Bind<ReadModelDispatcher>().ToSelf().InSingletonScope();


            //Bind<IDispatchCommits, IAsyncDispatchCommits, IRegisterEventHandlers>().To<EventDispatcher>().InSingletonScope();

            #endregion



            Bind<IStoreEvents, ICommitEvents, GSEventStore>().To<GSEventStore>().InSingletonScope();
            Bind<ISerialize>().To<JsonSerializer>();

            #endregion


            Bind<IGSRepository>().To<GSRepository>().InSingletonScope();
            Bind<IDispatchCommands>().To<CommandHandler>().InSingletonScope().OnActivation((_, x) =>
            {
                x.OtherHandlers[typeof(CreateUser)] = new List<Guid>() { GSAppState.GSAppId };
                x.OtherHandlers[typeof(CreatePlant)] = new List<Guid>() { GSAppState.GSAppId };
                x.OtherHandlers[typeof(BecomeFollower)] = new List<Guid>() { GSAppState.GSAppId };
            });
            Bind<ISynchronizerService>().To<SynchronizerService>().InSingletonScope();

            //Bind<IRegisterHandlers>().To<SynchronizerCommandHandler>().InSingletonScope();
            Bind<IAggregateFactory>().To<AggregateFactory>().InSingletonScope();



            Bind<IDetectConflicts>().To<ConflictDetector>().InSingletonScope();
            Bind<IJsonFactory>().To<JsonFactory>().InSingletonScope();


            //Bind<SynchronizerCommandHandler>().ToSelf().InSingletonScope();
            //Bind<ActionProjection>().ToSelf().InSingletonScope();
            //Bind<PlantProjection>().ToSelf().InSingletonScope();
            //Bind<IAuthTokenService>().To<AuthTokenService>().InSingletonScope();

            //RegisterHandlers(Kernel.Get<IMessageBus>(), Kernel);

            Kernel.Get<IMessageBus>().Listen<IAggregateCommand>().Subscribe(x =>
            {
                Kernel.Get<IDispatchCommands>().Handle(x);
            });

        }

        IDispatchCommits MBDispatcher;
        IDispatchCommits UIDispatcher;
        private void DoubleDispatch(Commit commit)
        {
            var mb = this.MBDispatcher ?? (this.MBDispatcher = Kernel.Get<MessageBusDispatcher>());
            var ui = this.UIDispatcher ?? (this.UIDispatcher = Kernel.Get<ReadModelDispatcher>());
            ui.Dispatch(commit);
            mb.Dispatch(commit);

        }




        void RegisterHandlers(IMessageBus bus, IKernel kernel)
        {
            // Bind<IAsyncEventHandler<UserSynchronized>>().To<AuthTokenService>().InSingletonScope();
            //var allEvents = bus.Listen<IEvent>();
            var allCommands = bus.Listen<IAggregateCommand>();

            var handler = Kernel.Get<IDispatchCommands>();
            allCommands.Subscribe(c => handler.Handle(c));


            //PlantProjection pproj = null;
            //ActionProjection aproj = null;
            //IDispatchCommands handler = null;

            //allEvents.Take(1).Subscribe(_ =>
            //{
            //    pproj = Kernel.Get<PlantProjection>();
            //    aproj = Kernel.Get<ActionProjection>();

            //    allEvents.OfType<Commented>().Subscribe(e => aproj.Handle(e));
            //    allEvents.OfType<Watered>().Subscribe(e => aproj.Handle(e));
            //    allEvents.OfType<Photographed>().Subscribe(e => aproj.Handle(e));
            //    allEvents.OfType<Fertilized>().Subscribe(e => aproj.Handle(e));



            //});

            //allCommands.Take(1).Subscribe(_ =>
            //{
            //    var handler = Kernel.Get<IDispatchCommands>();
            //    allCommands.Subscribe(c => handler.Handle(c));
            //    handler.Handle(_);

            //});




            //var aproj = Kernel.Get<ActionProjection>();
            //registry.Register<Commented>(aproj);
            //registry.Register<Watered>(aproj);
            //registry.Register<Photographed>(aproj);
            //registry.Register<Fertilized>(aproj);
            //registry.RegisterAsync<UserSynchronized>(Kernel.Get<AuthTokenService>());


            //var PVM = kernel.Get<PlantViewModel>();
            //kernel.Get<IMessenger>().Register<ShowPlantView>(PVM, PVM.Handle);

        }

    }




}


