using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Collections.Generic;


namespace Growthstories.Domain.Entities
{

    public enum RelationshipType
    {
        FOLLOWER,
        FRIEND
    }

    public struct Relationship
    {
        public readonly Guid Follower;
        public readonly Guid Target;
        public RelationshipType Type;
        public Relationship(Guid follower, Guid target, RelationshipType type)
        {
            this.Follower = follower;
            this.Target = target;
            this.Type = type;
        }
    }

    public class UserState : AggregateState<UserCreated>, IAuthUser
    {

        private IDictionary<Guid, RelationshipType> Relationships = new Dictionary<Guid, RelationshipType>();

        public string Password { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }

        public string AccessToken { get; private set; }
        public int ExpiresIn { get; private set; }
        public string RefreshToken { get; private set; }


        public Guid GardenId { get; private set; }

        public UserState() { }

        public new void Apply(UserCreated @event)
        {
            base.Apply(@event);
            this.Password = @event.Password;
            this.Username = @event.Username;
            this.Email = @event.Email;

        }

        public void Apply(BecameFollower @event)
        {
            this.Relationships[@event.OfUser] = RelationshipType.FOLLOWER;
        }

        public void Apply(AuthTokenSet @event)
        {
            this.AccessToken = @event.AccessToken;
            this.ExpiresIn = @event.ExpiresIn;
            this.RefreshToken = @event.RefreshToken;
        }


        public void Apply(GardenAdded @event)
        {
            this.GardenId = @event.GardenId;
        }


    }
}
