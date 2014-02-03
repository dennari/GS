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
using System.Diagnostics;

namespace Growthstories.Domain.Entities
{


    public sealed class AuthUser : IAuthUser
    {

        public const string UnregUsername = "UnregUser";
        public const string UnregEmailPrefix = "GSUnregUserEmail_";

        public string Username { get; set; }

        private string _Password;
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
                _Password = value;
            }
        }

        public string Email { get; set; }

        public Guid GardenId { get; set; }

        public string AccessToken { get; set; }

        public int ExpiresIn { get; set; }

        public string RefreshToken { get; set; }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public bool IsCollaborator { get; set; }

        public bool IsRegistered { get; set; }

        public bool LocationEnabled { get; set; }


    }


    public class GSAppState : AggregateState<GSAppCreated>, IGSAppState
    {

        public static readonly Guid GSAppId = new Guid("10000000-0000-0000-0000-000000000001");

        public readonly IDictionary<Guid, PullStream> SyncStreamDict = new Dictionary<Guid, PullStream>();

        private readonly IDictionary<string, Tuple<Photo, Guid>> _PhotoUploads = new Dictionary<string, Tuple<Photo, Guid>>();

        private readonly IDictionary<string, Tuple<Photo, Guid>> _PhotoDownloads = new Dictionary<string, Tuple<Photo, Guid>>();

        private readonly IDictionary<Guid, LocalPhotoPaths> _LocalPhotoPaths = new Dictionary<Guid, LocalPhotoPaths>();


        public IDictionary<string, Tuple<Photo, Guid>> PhotoUploads
        {
            get
            {
                return _PhotoUploads;
            }
        }

        public IDictionary<string, Tuple<Photo, Guid>> PhotoDownloads
        {
            get
            {
                return _PhotoDownloads;
            }
        }

        public IDictionary<Guid, LocalPhotoPaths> LocalPhotoPaths
        {
            get
            {
                return _LocalPhotoPaths;
            }
        }

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

        public GSLocation LastLocation { get; set; }


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
            //if (SyncStreamDict.ContainsKey(@event.StreamId))
            //    throw DomainError.Named("duplicate_syncstreams", "Stream already exists");
            SyncStreamDict[@event.StreamId] = new PullStream(@event.StreamId, @event.SyncStreamType, @event.AncestorId);
        }


        public void Apply(SyncStreamDeleted @event)
        {
            if (SyncStreamDict.ContainsKey(@event.StreamId))
                SyncStreamDict.Remove(@event.StreamId);

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
            _PhotoUploads[@event.Photo.LocalFullPath] = Tuple.Create(@event.Photo, @event.PlantActionId);
        }

        public void Apply(PhotoUploadCompleted @event)
        {
            _PhotoUploads.Remove(@event.Photo.LocalFullPath);
        }

        public void Apply(PhotoDownloadScheduled @event)
        {
            //if (@event.Photo.BlobKey == null)
            //    return;//throw DomainError.Named("no_blobkey", "To download a photo the BlobKey needs to be set.");
            _PhotoDownloads[@event.PlantActionId.ToString()] = Tuple.Create(@event.Photo, @event.PlantActionId);
        }

        public void Apply(PhotoDownloadCompleted @event)
        {
            //if (@event.Photo.BlobKey == null)
            //    return;//throw DomainError.Named("no_blobkey", "To download a photo the BlobKey needs to be set.");
            _PhotoDownloads.Remove(@event.PlantActionId.ToString());

            if (@event.Photo != null)
            {
                _LocalPhotoPaths[@event.PlantActionId] = new LocalPhotoPaths()
                {
                    LocalFullPath = @event.Photo.LocalFullPath,
                    LocalUri = @event.Photo.LocalUri,
                    FileName = @event.Photo.FileName
                };
            }
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


        public void Apply(InternalRegistered @event)
        {
            this._User.Email = @event.Email;
            this._User.Password = @event.Password;
            this._User.IsRegistered = true;
            this._User.Username = @event.Username;
        }


        public void Apply(AppUserAssigned @event)
        {
            // quite confusing that sometimes this._User points to
            // a UserState and sometimes to an AuthUser
            // -- JOJ 4.1.2014
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

        public void Apply(LocationEnabledSet @event)
        {
            this._User.LocationEnabled = @event.LocationEnabled;
        }

        public void Apply(PasswordSet @event)
        {
            this._User.Password = @event.Password;
        }

        public void Apply(EmailSet @event)
        {
            this._User.Email = @event.Email;
        }

        public void Apply(GardenAdded @event)
        {
            this._User.GardenId = @event.GardenId;
        }

        public void Apply(LocationAcquired @event)
        {
            this.LastLocation = @event.Location;
        }

    }
}
