using EventStore.Persistence;
using EventStore.Persistence.SqlPersistence;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Sync;
using Growthstories.UI;
using Growthstories.UI.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.DomainTests
{
    public class StagingModule : TestModule
    {

        protected override void HttpConfiguration()
        {
            Bind<IHttpClient, ITransportEvents, SyncHttpClient>().To<SyncHttpClient>().InSingletonScope();
            Bind<IEndpoint>().To<StagingEndpoint>();
            Bind<IRequestFactory, IResponseFactory>().To<RequestResponseFactory>().InSingletonScope();

        }




        protected override void PersistenceConfiguration()
        {
            Bind<IPersistSyncStreams, IPersistStreams>()
                .To<SQLitePersistenceEngine>()
                .InSingletonScope()
                .OnActivation((ctx, eng) =>
                {
                    eng.ReInitialize();
                });


            Bind<IUIPersistence>().To<SQLiteUIPersistence>()
                .InSingletonScope()
                .OnActivation((ctx, eng) =>
                {
                    eng.ReInitialize();
                });

        }


    }
}
