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
        Tuple<IEvent, SyncHead> GetNextPushEvent(SyncHead currentSyncHead);
        ISyncPushRequest CreatePushRequest(SyncHead syncHead);
        ISyncPushRequest CreateUserSyncRequest(Guid userId);
        ISyncPullRequest CreatePullRequest(ICollection<PullStream> streams);
        IPhotoUploadRequest CreatePhotoUploadRequest(Tuple<Photo, Guid> x);
        IPhotoDownloadRequest CreatePhotoDownloadRequest(Tuple<Photo, Guid> x);
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
        Task<Stream> OpenReadStream(string filename);
        Task<Tuple<Stream, string>> OpenWriteStream(string filename);
        Task<string> WriteToDisk(Stream readStream, string filename);
        string GetPhotoLocalUri(string filename);
        string GeneratePhotoFilename(string extension = "jpg");
        string FilenameFromBlobKey(string blobKey);
    }

    public class NullPhotoHandler : IPhotoHandler
    {


        public Task<Stream> OpenReadStream(string filename)
        {
            return Task.FromResult((Stream)null);
        }

        public Task<Tuple<Stream, string>> OpenWriteStream(string filename)
        {
            return Task.FromResult(Tuple.Create((Stream)null, (string)null));

        }

        public Task<string> WriteToDisk(Stream readStream, string filename)
        {
            return Task.FromResult((string)null);
        }


        public string GetPhotoLocalUri(string filename)
        {
            return filename;
        }

        public string GeneratePhotoFilename(string extension = "jpg")
        {
            return Guid.NewGuid() + "." + extension;
        }

        public string FilenameFromBlobKey(string blobKey)
        {
            return blobKey;
        }
    }


}
