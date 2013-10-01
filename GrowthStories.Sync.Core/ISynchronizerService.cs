

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Growthstories.Sync
{

    public class SyncResult
    {
        public IList<ISyncCommunication> Communication { get; set; }
    }


    public interface ISynchronizerService
    {
        Task<SyncResult> Synchronize();

        //IEnumerable<ISyncEventStream> Pending();
        ISyncPullRequest GetPullRequest();

        ISyncPushRequest GetPushRequest();

        ITransportEvents Transporter { get; }

        void MarkSynchronized(ISyncPushRequest pushReq, ISyncPushResponse pushResp = null);

        //IEnumerable<RebasePair> MatchStreams(ISyncPushRequest pushReq, ISyncPullResponse pullResp);

        //void Synchronized(ISyncPushRequest pushReq);

        Task TryAuth(ISyncPushRequest pushReq);
    }

    public struct RebasePair
    {

        public ISyncEventStream Local;
        public ISyncEventStream Remote;

        public RebasePair(ISyncEventStream local, ISyncEventStream remote)
        {
            this.Local = local;
            this.Remote = remote;
        }

    }
}
