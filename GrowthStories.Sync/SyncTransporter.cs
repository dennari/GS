using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SyncTransporter : ITransportEvents
    {
        public ISyncPushRequest CreatePushRequest(ICollection<IEventDTO> syncDTOs)
        {
            return new SyncPushRequest(syncDTOs);
        }

        public ISyncPullRequest CreatePullRequest()
        {
            throw new NotImplementedException();
        }

        public object CreatePushRequest(IEventDTO eventDTO)
        {
            throw new NotImplementedException();
        }
    }
}
