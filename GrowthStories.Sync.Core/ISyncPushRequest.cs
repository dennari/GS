using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace Growthstories.Sync
{

    public interface ISyncPushRequest : ISyncRequest
    {

        Guid ClientDatabaseId { get; }
        //Guid PushId { get; }

        ICollection<ISyncEventStream> Streams { get; }
        IEnumerable<IEventDTO> Events { get; }

    }


}