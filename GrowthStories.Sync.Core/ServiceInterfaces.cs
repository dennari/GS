using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;


namespace Growthstories.Sync
{
    public interface IRequestFactory
    {
        ISyncPushRequest CreatePushRequest(SyncHead syncHead);
        ISyncPushRequest CreateUserSyncRequest(Guid userId);
        ISyncPullRequest CreatePullRequest(ICollection<PullStream> streams);
        IPhotoUploadRequest CreatePhotoUploadRequest(Tuple<Photo, Guid> x);
        IPhotoDownloadRequest CreatePhotoDownloadRequest(Photo x);
    }



    public interface IResponseFactory
    {

        ISyncPullResponse CreatePullResponse(ISyncPullRequest req, Tuple<HttpResponseMessage, string> resp);
        ISyncPushResponse CreatePushResponse(ISyncPushRequest req, Tuple<HttpResponseMessage, string> resp);
        IPhotoUploadResponse CreatePhotoUploadResponse(IPhotoUploadRequest req, Tuple<HttpResponseMessage, string> resp);
        IPhotoDownloadResponse CreatePhotoDownloadResponse(IPhotoDownloadRequest req, Tuple<HttpResponseMessage, Stream> resp);

        IAuthResponse CreateAuthResponse(Tuple<HttpResponseMessage, string> resp);
        IUserListResponse CreateUserListResponse(Tuple<HttpResponseMessage, string> resp);
        RemoteUser CreateUserInfoResponse(Tuple<HttpResponseMessage, string> resp);
        //IPhotoUriResponse CreatePhotoUploadUriResponse(Tuple<HttpResponseMessage, string> resp);

        APIRegisterResponse CreateRegisterResponse(Tuple<HttpResponseMessage, string> resp);

    }


    public interface IPhotoHandler
    {
        Task<Stream> ReadPhoto(Photo photo);
        Task<Stream> WritePhoto(Photo photo);
    }


}
