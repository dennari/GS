using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Growthstories.Sync
{
    public interface IEndpoint
    {
        string Name { get; }
        Uri PullUri { get; }
        Uri PushUri { get; }
        Uri AuthUri { get; }
        Uri ClearDBUri { get; }

        Uri ShareUri(Guid userId, Guid plantId);
        Uri UserListUri(string username);
        Uri UserInfoUri(string email);
        Uri PhotoDownloadUri(string blobKey);

        Uri PhotoUploadUri { get; }
        Uri RegisterUri(string username, string email, string password);

    }

    public class Endpoint : IEndpoint
    {
        private readonly Uri BaseUri;

        public Endpoint(Uri baseUri)
        {
            BaseUri = baseUri;
        }

        public string Name
        {
            get { return "Staging"; }
        }

        public Uri PushUri
        {
            get
            {
                return new Uri(BaseUri, "/api/push");
            }
        }

        public Uri PullUri
        {
            get
            {
                return new Uri(BaseUri, "/api/pull");
            }
        }


        public Uri AuthUri
        {
            get
            {
                return new Uri(BaseUri, "/api/auth");
            }
        }


        public Uri ClearDBUri
        {
            get { return new Uri(BaseUri, "/api/test/clearDS"); }
        }


        public Uri UserListUri(string username)
        {
            return new Uri(BaseUri, string.Format("/api/user/list?prefix={0}", Uri.EscapeDataString(username)));
        }

        public Uri UserInfoUri(string email)
        {
            return new Uri(BaseUri, string.Format("/api/user/email/{0}", Uri.EscapeDataString(email)));
        }

        public Uri RegisterUri(string username, string email, string password)
        {
            return new Uri(BaseUri, string.Format(
               "/api/register?username={0}&email={1}&password={2}", 
               Uri.EscapeDataString(username), 
               Uri.EscapeDataString(email),
               Uri.EscapeDataString(password)
            ));
        }

        public Uri PhotoUploadUri
        {
            get { return new Uri(BaseUri, "/api/photo/uploadUrl"); }
        }

        public Uri PhotoDownloadUri(string blobKey)
        {
            return new Uri(BaseUri, string.Format("/api/photo/imageurl?blobKey={0}", blobKey));
        }

        public Uri ShareUri(Guid userId, Guid plantId)
        {
            return new Uri(BaseUri, string.Format("/plant/{0}/{1}", userId, plantId));
        }
    }
}
