using System;
using System.Collections.Generic;


namespace Growthstories.Sync
{
    public interface ITransportEvents
    {
        ISyncPushRequest CreatePushRequest(ICollection<IEventDTO> syncDTOs);
        ISyncPullRequest CreatePullRequest();

        //object CreatePushRequest(IEventDTO eventDTO);
    }
}
