using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Growthstories.Sync
{
    public interface ITransportEvents
    {
        Task<ISyncPushResponse> PushAsync(ISyncPushRequest request);
        Task<ISyncPullResponse> PullAsync(ISyncPullRequest request);
    }
}
