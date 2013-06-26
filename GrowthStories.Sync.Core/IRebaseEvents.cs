using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface IRebaseEvents
    {
        ISyncPushRequest Rebase(ISyncPushRequest pushReq, ISyncPullResponse pullResp);
        IEnumerable<ISyncEventStream> Pending();
    }
}
