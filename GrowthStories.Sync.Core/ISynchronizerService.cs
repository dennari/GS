

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
        IEnumerable<PullStream> SyncStreams { get; }
        IAuthUser User { get; }
    }

    public interface IGSApp : IGSAggregate
    {
        //IGSAppState State { get; }
        //bool CanHandle(IMessage msg);
    }

    public enum AllSyncResult
    {
        AllSynced,
        SomeLeft,
        Error
    }

    public interface ISyncInstance
    {
        ISyncPullRequest PullReq { get; }
        ISyncPushRequest PushReq { get; }
        ISyncPullResponse PullResp { get; }
        ISyncPushResponse PushResp { get; }
        IPhotoUploadRequest[] PhotoUploadRequests { get; }
        IPhotoDownloadRequest[] PhotoDownloadRequests { get; }

        Task<ISyncPullResponse> Pull();
        Task<ISyncPushResponse> Push();
        Task<IPhotoUploadResponse[]> UploadPhotos();
        Task<IPhotoDownloadResponse[]> DownloadPhotos(IPhotoDownloadRequest[] downloadRequests = null);
        int Merge();
        //IGSAggregate HandleRemoteMessages(IAggregateMessages msgs);


    }

    public interface ISynchronizerService
    {
        ISyncInstance Synchronize(ISyncPullRequest aPullReq, ISyncPushRequest aPushReq);

    }


}
