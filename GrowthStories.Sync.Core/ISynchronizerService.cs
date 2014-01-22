

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonDomain;
using Growthstories.Core;
namespace Growthstories.Sync
{

    public class SyncResult
    {
        public IList<ISyncCommunication> Communication { get; set; }
        public IList<Tuple<ISyncPushRequest, ISyncPushResponse>> Pushes { get; set; }
        public IList<Tuple<ISyncPullRequest, ISyncPullResponse>> Pulls { get; set; }

    }

    public interface IGSAppState : IMemento
    {
        IEnumerable<PullStream> SyncStreams { get; }
        SyncHead SyncHead { get; }
        IDictionary<string, Tuple<Photo, Guid>> PhotoUploads { get; }
        IDictionary<string, Photo> PhotoDownloads { get; }
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
        GSStatusCode Code { get; set; }

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

    public interface ISynchronizer
    {
        Task<Tuple<AllSyncResult, GSStatusCode?>> SyncAll(IGSAppState appState, int maxRounds = 20);
        Task<GSStatusCode> PrepareAuthorizedUser(SyncHead head);
        IDisposable SubscribeForAutoSync(IGSAppState appState);

        Task<IDisposable> AcquireLock();

    }


}
