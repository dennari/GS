

using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Growthstories.Sync
{

    public class SyncResult
    {
        public IList<ISyncCommunication> Communication { get; set; }
        public IList<Tuple<ISyncPushRequest, ISyncPushResponse>> Pushes { get; set; }
        public IList<Tuple<ISyncPullRequest, ISyncPullResponse>> Pulls { get; set; }

    }

    public interface IGSAppState
    {
        IEnumerable<SyncStreamInfo> SyncStreams { get; }
    }

    public interface IGSApp : IGSAggregate
    {
        IGSAppState State { get; }
    }

    public interface ISynchronizerService
    {
        Task<SyncResult> Synchronize(IGSApp app, ISyncPullRequest aPullReq = null, ISyncPushRequest aPushReq = null);
        Task<bool> CreateUserAsync(Guid userId);
        //IEnumerable<ISyncEventStream> Pending();
        //ISyncPullRequest GetPullRequest();

        //ISyncPushRequest GetPushRequest();

        ITransportEvents Transporter { get; }

        //void MarkSynchronized(ISyncPushRequest pushReq, ISyncPushResponse pushResp = null);

        //IEnumerable<RebasePair> MatchStreams(ISyncPushRequest pushReq, ISyncPullResponse pullResp);

        //void Synchronized(ISyncPushRequest pushReq);

        //Task TryAuth(ISyncPushRequest pushReq);
    }


}
