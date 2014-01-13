
using EventStore.Logging;
using EventStore.Persistence;
using EventStore.Persistence.SqlPersistence;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Services;
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
        //public StagingEndpoint() : base(new Uri("http://gs-devel.appspot.com")) { }
        //public StagingEndpoint() : base(new Uri("http://192.168.0.51:8080")) { }
        public StagingEndpoint() : base(new Uri("http://dennari-macbook.lan:8080")) { }
        //public StagingEndpoint() : base(new Uri("https://gs-prod.appspot.com")) { }
    }


    public class Bootstrap : BaseSetup
    {

        public override void Load()
        {
            base.Load();
        }


        protected override void HttpConfiguration()
        {
            Bind<IHttpClient, ITransportEvents, SyncHttpClient>().To<SyncHttpClient>().InSingletonScope();
            Bind<IEndpoint>().To<StagingEndpoint>();
            Bind<IRequestFactory, RequestFactory>().To<RequestFactory>().InSingletonScope();
            Bind<IResponseFactory, ResponseFactory>().To<ResponseFactory>().InSingletonScope();

        }

        protected override void FileSystemConfiguration()
        {
            Bind<IPhotoHandler>().To<WP8PhotoHandler>();
        }

        protected override void SQLiteConnectionConfiguration()
        {
            SQLiteConnection conn = null;
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "GS.sqlite");
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

        protected override void PersistenceConfiguration()
        {
            Bind<IPersistSyncStreams, IPersistStreams>().To<SQLitePersistenceEngine>().InSingletonScope();
            Bind<IUIPersistence>().To<SQLiteUIPersistence>().InSingletonScope();
        }

        //protected override void LogConfiguration()
        //{
        //    if (System.Diagnostics.Debugger.IsAttached)
        //        LogFactory.BuildLogger = type => new DebuggerLog(type);
        //}

    }


}


