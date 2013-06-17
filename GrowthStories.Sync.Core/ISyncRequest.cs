using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace Growthstories.Sync
{
    public interface ISyncRequest
    {

    }

    public interface ISyncPushRequest : ISyncRequest
    {

        Guid ClientDatabaseId { get; }
        Guid PushId { get; }
        IEnumerable<IEventDTO> Events { get; }

        Task<ISyncPushResponse> ExecuteAsync();

    }

    public interface ISyncPullRequest : ISyncRequest
    {
        Task<ISyncPullResponse> ExecuteAsync();
    }





}