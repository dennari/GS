
using EventStore.Logging;
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



        protected override void PersistenceConfiguration()
        {
            Bind<IPersistSyncStreams, IPersistStreams>().To<SQLitePersistenceEngine>().InSingletonScope().OnActivation((ctx, eng) =>
                {
                    eng.ReInitialize();
                });
            Bind<IUIPersistence>().To<SQLiteUIPersistence>().InSingletonScope().OnActivation((ctx, eng) =>
            {
                eng.ReInitialize();
            });


        }

        protected override void LogConfiguration()
        {

            if (System.Diagnostics.Debugger.IsAttached)
                LogFactory.BuildLogger = type => new DebuggerLog(type);

        }

    }

    class DebuggerLog : ILog
    {
        private Type type;

        public DebuggerLog(Type type)
        {
            // TODO: Complete member initialization
            this.type = type;
        }

        protected void WriteToConsole(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        protected string Tag(string m)
        {
            if (type.FullName.Contains("Growthstories") || type == typeof(SQLitePersistenceEngine))
                return "[GS] " + m;
            else
                return m;
        }

        public void Verbose(string message, params object[] values)
        {
            try
            {
                WriteToConsole(Tag(string.Format(message, values)));

            }
            catch (Exception)
            {
                WriteToConsole(Tag(message));
            }
        }

        public void Debug(string message, params object[] values)
        {
            try
            {
                WriteToConsole(Tag(string.Format(message, values)));

            }
            catch (Exception)
            {
                WriteToConsole(Tag(message));
            }

        }

        public void Info(string message, params object[] values)
        {

            try
            {
                WriteToConsole(Tag(string.Format(message, values)));

            }
            catch (Exception)
            {
                WriteToConsole(Tag(message));
            }


        }

        public void Warn(string message, params object[] values)
        {

            try
            {
                WriteToConsole(Tag(string.Format(message, values)));

            }
            catch (Exception)
            {
                WriteToConsole(Tag(message));
            }


        }

        public void Error(string message, params object[] values)
        {

            try
            {
                WriteToConsole(Tag(string.Format(message, values)));

            }
            catch (Exception)
            {
                WriteToConsole(Tag(message));
            }


        }

        public void Fatal(string message, params object[] values)
        {

            try
            {
                WriteToConsole(Tag(string.Format(message, values)));

            }
            catch (Exception)
            {
                WriteToConsole(Tag(message));
            }

        }
    }


}


