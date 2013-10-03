using Growthstories.Core;
using Growthstories.DomainTests.Staging;
using Growthstories.Sync;
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

        protected override void UserConfiguration()
        {
            Bind<IUserService>().To<StagingUserService>().InSingletonScope();
        }

        protected override void EventFactoryConfiguration()
        {
            Bind<IEventFactory>().To<EventFactory>().InSingletonScope();
        }

    }
}
