using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Growthstories.Sync
{
    public class FakeSyncPushRequest : SyncPushRequest
    {
        public new Task<ISyncPushResponse> Execute()
        {
            throw new NotImplementedException();
        }
    }
}
