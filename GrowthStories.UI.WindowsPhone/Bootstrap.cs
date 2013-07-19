
using EventStore.Persistence;
using EventStore.Persistence.SqlPersistence;
using GalaSoft.MvvmLight.Messaging;
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

namespace GrowthStories.UI.WindowsPhone
{
    public class Bootstrap : TestModule
    {
        public override void Load()
        {
            base.Load();


        }

        protected override void UIConfiguration()
        {
            Bind<IMessenger>().To<Messenger>().InSingletonScope();
            Bind<INavigationService>().To<NavigationService>().InSingletonScope();
            Bind<GardenViewModel>().ToSelf().InSingletonScope();
            Bind<PlantViewModel>().ToSelf().InSingletonScope();
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
                    eng.Initialize();
                    //eng.Purge();
                });
            Bind<IUIPersistence>().To<SQLiteUIPersistence>().InSingletonScope().OnActivation((ctx, eng) =>
            {
                eng.Initialize();
                //eng.Purge();
            });


        }

    }


}


