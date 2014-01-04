using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using System.Linq;
using Growthstories.Core;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{



    #region GSApp
    public sealed class CreateGSApp : AggregateCommand<GSApp>, ICreateMessage
    {



        //protected CreateGSApp() { }
        public CreateGSApp()
            : base(GSAppState.GSAppId)
        {

        }

        public override string ToString()
        {
            return string.Format(@"Create GSApp {0}.", EntityId);
        }

    }


    public sealed class InternalRegisterAppUser : AggregateCommand<GSApp>
    {
        public readonly Guid UserId;
        public readonly string Username;
        public readonly string Password;
        public readonly string Email;

        public InternalRegisterAppUser(Guid userId, string username, string password, string email)
            : base(GSAppState.GSAppId)
        {
            this.UserId = userId;
            this.Username = username;
            this.Password = password;
            this.Email = email;
        }

       public override string ToString()
        {
            return string.Format(@"Internally mark user registered", UserId);
        }

    }

    public sealed class AssignAppUser : AggregateCommand<GSApp>
    {
        public readonly Guid UserId;
        public readonly string Username;
        public readonly string Password;
        public readonly string Email;

        public Guid UserGardenId;

        public int UserVersion;

        public AssignAppUser(Guid userId, string username, string password, string email)
            : base(GSAppState.GSAppId)
        {
            this.UserId = userId;
            this.Username = username;
            this.Password = password;
            this.Email = email;
        }

        public override string ToString()
        {
            return string.Format(@"Assign user {0} to app.", UserId);
        }

    }

    public sealed class LogOutAppUser : AggregateCommand<GSApp>
    {
        public readonly Guid UserId;

        public LogOutAppUser(Guid userId)
            : base(GSAppState.GSAppId)
        {
            this.UserId = userId;

        }

        public override string ToString()
        {
            return string.Format(@"Logout user {0}", UserId);
        }

    }



    public sealed class SetAuthToken : AggregateCommand<GSApp>
    {


        public string AccessToken { get; private set; }
        public int ExpiresIn { get; private set; }
        public string RefreshToken { get; private set; }


        public SetAuthToken(string accessToken, string refreshToken, int expiresIn)
            : base(GSAppState.GSAppId)
        {
            this.AccessToken = accessToken;
            this.ExpiresIn = expiresIn;
            this.RefreshToken = refreshToken;
        }

        public SetAuthToken(IAuthToken auth)
            : this(auth.AccessToken, auth.RefreshToken, auth.ExpiresIn)
        {

        }

        public override string ToString()
        {
            return string.Format(@"SetAuthToken access: {0}, refresh: {1}, expires {2}, for user {3}.", AccessToken, RefreshToken, ExpiresIn, AggregateId);
        }

    }


    public sealed class CreateSyncStream : AggregateCommand<GSApp>
    {

        public readonly Guid StreamId;

        public readonly PullStreamType SyncStreamType;



        public CreateSyncStream(Guid streamId, PullStreamType type, Guid? ancestorId = null)
            : base(GSAppState.GSAppId)
        {
            this.StreamId = streamId;
            this.SyncStreamType = type;
            this.AncestorId = ancestorId;
        }



        public override string ToString()
        {
            return string.Format(@"Add syncstream {0} of type {1}", StreamId, SyncStreamType);
        }


    }

    public sealed class SetSyncStamp : AggregateCommand<GSApp>
    {

        public readonly Guid StreamId;

        public readonly long SyncStamp;


        public SetSyncStamp(Guid streamId, long syncStamp)
            : base(GSAppState.GSAppId)
        {
            this.StreamId = streamId;
            this.SyncStamp = syncStamp;
        }



        public override string ToString()
        {
            return string.Format(@"Set syncstamp to {0} for stream {1}", SyncStamp, StreamId);
        }


    }


    public sealed class SchedulePhotoUpload : AggregateCommand<GSApp>
    {


        public readonly Photo Photo;
        public Guid PlantActionId { get; private set; }


        public SchedulePhotoUpload(Photo photo, Guid plantActionId)
            : base(GSAppState.GSAppId)
        {
            this.Photo = photo;
            this.PlantActionId = plantActionId;
        }



        public override string ToString()
        {
            return string.Format(@"Schedule new photo upload");
        }


    }

    public sealed class CompletePhotoUpload : AggregateCommand<GSApp>
    {


        public readonly Photo Photo;
        public readonly Guid PlantActionId;
        public readonly string BlobKey;

        public CompletePhotoUpload(IPhotoUploadResponse response)
            : base(GSAppState.GSAppId)
        {
            this.Photo = response.Photo;
            this.PlantActionId = response.PlantActionId;
            this.BlobKey = response.BlobKey;
        }



        public override string ToString()
        {
            return string.Format(@"Complete photo upload");
        }


    }

    public sealed class CompletePhotoDownload : AggregateCommand<GSApp>
    {


        public readonly Photo Photo;


        public CompletePhotoDownload(Photo photo)
            : base(GSAppState.GSAppId)
        {
            this.Photo = photo;
        }



        public override string ToString()
        {
            return string.Format(@"Complete photo download");
        }


    }

    public abstract class Synchronize : AggregateCommand<GSApp>
    {
        public int LastGlobalCommitSequence { get; protected set; }
        public ISyncInstance Sync { get; protected set; }

        public Synchronize(ISyncInstance s)
            : base(GSAppState.GSAppId)
        {
            this.Sync = s;
        }

        public override string ToString()
        {
            return string.Format(@"Synchronize.");
        }

    }

    public sealed class Push : Synchronize
    {

        public SyncHead SyncHead { get; private set; }


        public Push(ISyncInstance s)
            : base(s)
        {
            SyncHead = s.PushReq.SyncHead;
            //this.NumEventsPushed = s.PushReq.Streams.Aggregate(0, (acc, x) => acc + x.Count);
        }


        public override string ToString()
        {
            return string.Format(@"Push.");
        }

    }

    public sealed class Pull : Synchronize
    {


        public Pull(ISyncInstance s) : base(s) { }


        public override string ToString()
        {
            return string.Format(@"Pull.");
        }

    }

    #endregion


}

