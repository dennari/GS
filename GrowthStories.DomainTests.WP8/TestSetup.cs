using EventStore.Persistence;
using EventStore.Persistence.SqlPersistence;
using Growthstories.Domain;
using Growthstories.Sync;
using Growthstories.UI.Persistence;

namespace Growthstories.DomainTests
{
    public class TestModule : BaseSetup
    {



        protected override void FileSystemConfiguration()
        {
            Bind<IPhotoHandler>().To<WP8PhotoHandler>();

        }


    }

    public class SyncEngineTestsSetup : TestModule
    {

        protected override void HttpConfiguration()
        {
            Bind<IHttpClient, ITransportEvents, FakeHttpClient>().To<FakeHttpClient>().InSingletonScope();
            Bind<IEndpoint, FakeEndpoint>().To<FakeEndpoint>().InSingletonScope();
            Bind<IRequestFactory, RequestFactory>().To<RequestFactory>().InSingletonScope();
            Bind<IResponseFactory, ResponseFactory>().To<ResponseFactory>().InSingletonScope();


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

        protected override void UserConfiguration()
        {
            Bind<IUserService>().To<TestUserService>().InSingletonScope();
        }




    }
}


