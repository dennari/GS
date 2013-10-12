
using EventStore.Persistence;
using EventStore.Persistence.SqlPersistence;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Services;
using Growthstories.DomainTests;
using Growthstories.Sync;
using Growthstories.UI;
using Growthstories.UI.Persistence;
using Growthstories.UI.ViewModel;
using Ninject.Modules;
using SQLite;
using System;
using System.IO;
using Windows.Storage;

namespace Growthstories.UI.WindowsPhone
{

    class StagingEndpoint : Endpoint
    {
        public StagingEndpoint() : base(new Uri("http://server.lan:9000")) { }
    }
    public class Bootstrap : TestModule
    {
        public override void Load()
        {
            base.Load();


        }

        protected override void HttpConfiguration()
        {
            Bind<IHttpClient, ITransportEvents, SyncHttpClient>().To<SyncHttpClient>().InSingletonScope();
            Bind<IEndpoint>().To<StagingEndpoint>();
            Bind<IRequestFactory, IResponseFactory>().To<RequestResponseFactory>().InSingletonScope();

        }

        protected override void UIConfiguration()
        {
            //Bind<INavigationService>().To<NavigationService>().InSingletonScope();
            //Bind<GardenViewModel>().ToSelf().InSingletonScope();
            //Bind<AddPlantViewModel>().ToSelf().InSingletonScope();
            //Bind<PlantViewModel>().ToSelf().InSingletonScope();
            //Bind<MainViewModel>().ToSelf().InSingletonScope();
        }

        protected override void SQLiteConnectionConfiguration()
        {
            SQLiteConnection conn = null;
            Func<SQLiteConnection> del = () =>
            {
                if (conn == null)
                {
                    conn = new SQLiteConnection(Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "sample.sqlite")));

                }
                return conn;
            };
            Bind<ISQLiteConnectionFactory>().To<DelegateConnectionFactory>().WithConstructorArgument("f", (object)del);
        }


        protected override void EventFactoryConfiguration()
        {
            Bind<IEventFactory>().To<EventFactory>().InSingletonScope();
        }


        protected override void PersistenceConfiguration()
        {
            Bind<IPersistSyncStreams, IPersistStreams>().To<SQLitePersistenceEngine>().InSingletonScope().OnActivation((ctx, eng) =>
                {
                    eng.ReInitialize();
                });
            Bind<IUIPersistence>().To<SQLiteUIPersistence>().InSingletonScope().OnActivation((ctx, eng) =>
            {
                eng.Initialize();
                eng.Purge();
            });


        }

    }


}


