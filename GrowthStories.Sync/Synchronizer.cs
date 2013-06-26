using System;
using System.Linq;
using CommonDomain.Core;
using CommonDomain;
using EventStore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Growthstories.Domain.Entities;
using Growthstories.Core;
using System.Net.Http;
using Growthstories.Domain.Messaging;

namespace Growthstories.Sync
{
    public class Synchronizer
    {

        private readonly ITransportEvents Transporter;
        private readonly IRequestFactory RequestFactory;
        private readonly IRebaseEvents Rebaser;




        public Synchronizer(
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IRebaseEvents rebaser
            )
        {
            Transporter = transporter;
            RequestFactory = requestFactory;
            Rebaser = rebaser;
        }


        public async Task<int> Synchronize(ISyncPushRequest pushReq = null)
        {
            if (pushReq == null)
                pushReq = GetPushRequest();
            if (pushReq == null)
                return 0;

            int MaxTries = 5;
            int Counter = 0;

            ISyncPushResponse pushResp;
            ISyncPullRequest pullReq;
            ISyncPullResponse pullResp;


            do
            {

                pushResp = await Transporter.PushAsync(pushReq);
                Counter++;

                if (pushResp.StatusCode == 200)
                    break;

                // pull
                pullReq = RequestFactory.CreatePullRequest(pushReq.Streams);
                pullResp = await Transporter.PullAsync(pullReq);
                if (pullResp.Streams.Count > 0)
                {
                    pushReq = Rebaser.Rebase(pushReq, pullResp);
                }

            } while (Counter < MaxTries);


            return Counter;
        }

        private ISyncPushRequest GetPushRequest()
        {
            var pending = Rebaser.Pending().ToArray();
            if (pending.Length == 0)
                return null;

            return RequestFactory.CreatePushRequest(pending);
        }


    }
}
