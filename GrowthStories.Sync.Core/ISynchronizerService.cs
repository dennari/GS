

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


    public enum SyncStatus
    {
        OK,
        PULL_ERROR,
        PULL_HANDLE_ERROR,
        MERGE_ERROR,
        PUSH_ERROR,
        PHOTOUPLOAD_ERROR,
        PHOTODOWNLOAD_ERROR,
        AUTH_ERROR,
        PULL_EMPTY_ERROR
    }
    

    public interface ISyncInstance
    {
        
        /*IAuthResponse AuthResp { get; }*/

        SyncStatus Status { get; set; }
        GSStatusCode Code {get; set; }

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
