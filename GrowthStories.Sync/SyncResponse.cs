using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{



    public interface ISyncResponse
    {
        bool IsValid();
    }

    public interface ISyncPushResponse : ISyncResponse
    {

        Guid ClientDatabaseId { get; }
        Guid PushId { get; }
        IEnumerable<IEventDTO> Events { get; }
    }

    public interface ISyncPullResponse : ISyncResponse
    { }





}
