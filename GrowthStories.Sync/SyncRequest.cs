using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        ICollection<IEventDTO> Events { get; }

        Task<ISyncPushResponse> ExecuteAsync();

    }

    public interface ISyncPullRequest : ISyncRequest
    {
        Task<ISyncPullResponse> ExecuteAsync();
    }





}