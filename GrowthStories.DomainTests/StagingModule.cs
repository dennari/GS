﻿using Growthstories.Core;
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
            Bind<IHttpClient>().To<SyncHttpClient>().InSingletonScope();
            Bind<IEndpoint>().To<StagingEndpoint>();
            Bind<IRequestFactory, IResponseFactory>().To<RequestResponseFactory>().InSingletonScope();
            Bind<IHttpRequestFactory, IHttpResponseFactory>().To<HttpRequestResponseFactory>().InSingletonScope();

        }

        //protected override void UserConfiguration()
        //{
        //    Bind<IUserService>().To<SyncUserService>().InSingletonScope();
        //}

        protected override void EventFactoryConfiguration()
        {
            Bind<IEventFactory>().To<EventFactory>().InSingletonScope();
        }

    }
}