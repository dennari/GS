using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{



    #region GSApp
    public class CreateGSApp : AggregateCommand<GSApp>, ICreateMessage
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

    public class AssignAppUser : AggregateCommand<GSApp>
    {
        public readonly Guid UserId;
        public readonly string Username;
        public readonly string Password;
        public readonly string Email;

        public Guid UserGardenId;

        public int UserVersion;



        protected AssignAppUser() { }
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


    public class SetAuthToken : AggregateCommand<GSApp>
    {


        public string AccessToken { get; protected set; }
        public int ExpiresIn { get; protected set; }
        public string RefreshToken { get; protected set; }


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


    public class CreateSyncStream : AggregateCommand<GSApp>
    {

        public readonly Guid StreamId;

        public readonly Guid? AncestorId;

        public readonly StreamType StreamType;



        protected CreateSyncStream() { }
        public CreateSyncStream(Guid streamId, StreamType type, Guid? ancestorId = null)
            : base(GSAppState.GSAppId)
        {
            this.StreamId = streamId;
            this.StreamType = type;
            this.AncestorId = ancestorId;
        }



        public override string ToString()
        {
            return string.Format(@"Add syncstream {0} of type {1}", StreamId, StreamType);
        }


    }

    public class SetSyncStamp : AggregateCommand<GSApp>
    {

        public readonly Guid StreamId;

        public readonly long SyncStamp;


        protected SetSyncStamp() { }
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

    public abstract class Synchronize : AggregateCommand<GSApp>
    {
        public int LastGlobalCommitSequence { get; protected set; }
        public int GlobalCommitSequence { get; set; }
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

    public class Push : Synchronize
    {



        public Push(ISyncInstance s)
            : base(s)
        {
            LastGlobalCommitSequence = s.PushReq.GlobalCommitSequence;
        }


        public override string ToString()
        {
            return string.Format(@"Push.");
        }

    }

    public class Pull : Synchronize
    {


        public Pull(ISyncInstance s) : base(s) { }


        public override string ToString()
        {
            return string.Format(@"Pull.");
        }

    }

    #endregion


}

