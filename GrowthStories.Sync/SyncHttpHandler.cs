using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SyncHttpHandler : MessageProcessingHandler
    {

        public SyncHttpHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return response;
        }

        //protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        //{

        //}
    }
}
