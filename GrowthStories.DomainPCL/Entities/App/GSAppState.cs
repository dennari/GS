using CommonDomain;
using EventStore;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Entities
{


    public sealed class AuthUser : IAuthUser
    {

        public const string UnregEmailPrefix = "GSUnregUserEmail_";

        public string Username { get; set; }


        public string Password { get; set; }

        public string Email { get; set; }


        public Guid GardenId { get; set; }


        public string AccessToken { get; set; }


        public int ExpiresIn { get; set; }


        public string RefreshToken { get; set; }


        public Guid Id { get; set; }


        public int Version { get; set; }

        public bool IsCollaborator { get; set; }



        public bool IsRegistered()
        {
            return Email != null && !Email.StartsWith(AuthUser.UnregEmailPrefix);
        }
    }


    public class GSAppState : AggregateState<GSAppCreated>, IGSAppState
    {

        public static readonly Guid GSAppId = new Guid("10000000-0000-0000-0000-000000000001");

        public readonly IDictionary<Guid, PullStream> SyncStreamDict = new Dictionary<Guid, PullStream>();

        public readonly IDictionary<string, Photo> PhotoUploads = new Dictionary<string, Photo>();

        public readonly IDictionary<string, Photo> PhotoDownloads = new Dictionary<string, Photo>();


        public IEnumerable<PullStream> SyncStreams
        {
            get
            {
                return SyncStreamDict.Select(x => x.Value);
            }
        }

        protected AuthUser _User;

        public IAuthUser User { get { return _User; } }

        public SyncHead SyncHead { get; protected set; }

        public GSAppState()
            : base()
        {

            this.SyncHead = new SyncHead(0, 0, 0);
        }

        public GSAppState(GSAppCreated e)
            : this()
        {
            this.Apply(e);

        }


        public override void Apply(GSAppCreated @event)
        {

            if (@event.AggregateId != GSAppId)
                throw new ArgumentException(string.Format("There can only be a single GSApp aggregate per installation and it has to be assigned id {0}", GSAppId));
            base.Apply(@event);
        }

        public void Apply(SyncStreamCreated @event)
        {
            if (SyncStreamDict.ContainsKey(@event.StreamId))
                throw DomainError.Named("duplicate_syncstreams", "Stream already exists");
            SyncStreamDict[@event.StreamId] = new PullStream(@event.StreamId, @event.SyncStreamType, @event.AncestorId);
        }

        //public void Apply(SyncStampSet @event)
        //{
        //    PullStream syncStream = null;
        //    if (SyncStreamDict.TryGetValue(@event.StreamId, out syncStream))
        //    {
        //        syncStream.NextSince = @event.SyncStamp;
        //    }
        //    else
        //    {
        //        throw DomainError.Named("syncstream_missing", "Tried to set syncstamp for missing syncstream");
        //    }
        //}        //public void Apply(SyncStampSet @event)
        //{
        //    PullStream syncStream = null;
        //    if (SyncStreamDict.TryGetValue(@event.StreamId, out syncStream))
        //    {
        //        syncStream.NextSince = @event.SyncStamp;
        //    }
        //    else
        //    {
        //        throw DomainError.Named("syncstream_missing", "Tried to set syncstamp for missing syncstream");
        //    }
        //}

        public void Apply(PhotoUploadScheduled @event)
        {
            PhotoUploads[@event.Photo.LocalFullPath] = @event.Photo;
        }

        public void Apply(PhotoUploadCompleted @event)
        {
            PhotoUploads.Remove(@event.Photo.LocalFullPath);
        }

        public void Apply(PhotoDownloadScheduled @event)
        {
            if (@event.Photo.BlobKey == null)
                throw DomainError.Named("no_blobkey", "To download a photo the BlobKey needs to be set.");
            PhotoDownloads[@event.Photo.BlobKey] = @event.Photo;
        }

        public void Apply(PhotoDownloadCompleted @event)
        {
            if (@event.Photo.BlobKey == null)
                throw DomainError.Named("no_blobkey", "To download a photo the BlobKey needs to be set.");
            PhotoDownloads.Remove(@event.Photo.BlobKey);
        }


        public void Apply(Pulled @event)
        {
            //SyncSequence = @event.SyncSequence;

            foreach (var kv in @event.SyncStamps)
            {
                PullStream syncStream = null;
                if (SyncStreamDict.TryGetValue(kv.Key, out syncStream))
                {
                    syncStream.Since = kv.Value;
                }
            }
        }

        public void Apply(Pushed @event)
        {
            SyncHead = @event.SyncHead;
        }



        public void Apply(AppUserAssigned @event)
        {
            this._User = new AuthUser()
            {
                Id = @event.UserId,
                GardenId = @event.UserGardenId,
                Version = @event.UserVersion,
                Username = @event.Username,
                Password = @event.Password,
                Email = @event.Email
            };
        }

        public void Apply(AppUserLoggedOut @event)
        {
            if (this.User != null && this.User.Id == @event.UserId)
            {
                this._User = null;
            }
        }

        public void Apply(AuthTokenSet @event)
        {
            this._User.AccessToken = @event.AccessToken;
            this._User.ExpiresIn = @event.ExpiresIn;
            this._User.RefreshToken = @event.RefreshToken;
        }

        public void Apply(UsernameSet @event)
        {

            this._User.Username = @event.Username;

        }

        public void Apply(PasswordSet @event)
        {

            this._User.Password = @event.Password;

        }

        public void Apply(EmailSet @event)
        {

            this._User.Email = @event.Email;

        }


    }
}
