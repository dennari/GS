using CommonDomain;
using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Growthstories.Sync
{
    public interface ISyncPullResponse : ISyncResponse
    {
        //IEnumerable<IEvent> Events { get; }
        ICollection<ISyncEventStream> Streams { get; }

    }
}
