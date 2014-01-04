using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Growthstories.Sync
{
    public interface ITransportEvents
    {
        IAuthToken AuthToken { get; set; }
        Task<ISyncPushResponse> PushAsync(ISyncPushRequest request);
        Task<ISyncPullResponse> PullAsync(ISyncPullRequest request);
        Task<IAuthResponse> RequestAuthAsync(string username, string password);

        Task<IUserListResponse> ListUsersAsync(string username);
        Task<RemoteUser> UserInfoAsync(string email);

        Task<IPhotoUriResponse> RequestPhotoUploadUri();
        Task<IPhotoUriResponse> RequestPhotoDownloadUri(string blobKey);

        Task<IPhotoUploadResponse> RequestPhotoUpload(IPhotoUploadRequest request);
        Task<IPhotoDownloadResponse> RequestPhotoDownload(IPhotoDownloadRequest request);

        Task<APIRegisterResponse> RegisterAsync(string username, string email, string password);
    }
}
