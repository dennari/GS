

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Growthstories.Sync
{
    public interface ISynchronizerService
    {
        //Task<IList<ISyncRequest>> Synchronize();

        //IEnumerable<ISyncEventStream> Pending();
        ISyncPullRequest GetPullRequest();

        ISyncPushRequest GetPushRequest();

        ITransportEvents Transporter { get; }

        void MarkAllSynchronized(ISyncPushRequest pushReq);

        //IEnumerable<RebasePair> MatchStreams(ISyncPushRequest pushReq, ISyncPullResponse pullResp);
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
