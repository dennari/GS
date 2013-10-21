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

    }


    public class GSAppState : AggregateState<GSAppCreated>, IGSAppState
    {

        public static readonly Guid GSAppId = new Guid("10000000-0000-0000-0000-000000000001");

        public readonly IDictionary<Guid, SyncStreamInfo> SyncStreamDict;


        public IEnumerable<SyncStreamInfo> SyncStreams
        {
            get
            {
                return SyncStreamDict.Select(x => x.Value);
            }
        }

        protected AuthUser _User;

        public IAuthUser User { get { return _User; } }

        public int SyncSequence { get; protected set; }

        public GSAppState()
            : base()
        {
            this.SyncStreamDict = new Dictionary<Guid, SyncStreamInfo>();

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
            SyncStreamDict[@event.StreamId] = new SyncStreamInfo(@event.StreamId, @event.StreamType, @event.AncestorId);
        }

        public void Apply(SyncStampSet @event)
        {
            SyncStreamInfo syncStream = null;
            if (SyncStreamDict.TryGetValue(@event.StreamId, out syncStream))
            {
                syncStream.SyncStamp = @event.SyncStamp;
            }
            else
            {
                throw DomainError.Named("syncstream_missing", "Tried to set syncstamp for missing syncstream");
            }
        }

        public void Apply(Pulled @event)
        {
            SyncSequence = @event.SyncSequence;

            foreach (var syncStream in SyncStreamDict.Values)
            {
                syncStream.SyncStamp = @event.SyncStamp;
            }
        }

        public void Apply(Pushed @event)
        {
            SyncSequence = @event.SyncSequence;
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

        public void Apply(AuthTokenSet @event)
        {
            this._User.AccessToken = @event.AccessToken;
            this._User.ExpiresIn = @event.ExpiresIn;
            this._User.RefreshToken = @event.RefreshToken;
        }



    }
}
