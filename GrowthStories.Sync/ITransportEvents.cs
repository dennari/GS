using System;
using System.Collections.Generic;


namespace Growthstories.Sync
{
    public interface ITransportEvents
    {
        ISyncPushRequest CreatePushRequest(IEnumerable<IEventDTO> syncDTOs);
        ISyncPullRequest CreatePullRequest();
    }
}
