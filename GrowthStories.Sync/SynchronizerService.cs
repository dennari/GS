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
using EventStore.Persistence;
using Growthstories.Domain;
using System.Net;
using ReactiveUI;
using EventStore.Logging;

namespace Growthstories.Sync
{


    public sealed class SynchronizerService : ISynchronizerService
    {


        public SynchronizerService()
        {

        }



        public ISyncInstance Synchronize(ISyncPullRequest aPullReq, ISyncPushRequest aPushReq)
        {
            return new SyncInstance(aPullReq, aPushReq);
        }
    }



}
