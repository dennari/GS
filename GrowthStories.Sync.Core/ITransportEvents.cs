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


        Task<IPhotoUploadUriResponse> RequestPhotoUploadUri();
    }
}
