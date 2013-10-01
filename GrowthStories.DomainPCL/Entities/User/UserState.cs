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
        FRIEND_REQUEST,
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

        private IDictionary<Guid, Relationship> Relationships = new Dictionary<Guid, Relationship>();

        public IDictionary<Guid, ScheduleState> Schedules { get; private set; }

        public IDictionary<Guid, GardenState> Gardens { get; private set; }

        public string Password { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }

        public string AccessToken { get; private set; }
        public int ExpiresIn { get; private set; }
        public string RefreshToken { get; private set; }




        public UserState() { }

        public new void Apply(UserCreated @event)
        {
            base.Apply(@event);
            this.Password = @event.Password;
            this.Username = @event.Username;
            this.Email = @event.Email;

            this.Schedules = new Dictionary<Guid, ScheduleState>();
            this.Gardens = new Dictionary<Guid, GardenState>();

        }

        protected Guid _GardenId;
        public Guid GardenId
        {
            get { return _GardenId; }
            protected set
            {
                _GardenId = value;
            }
        }

        public void Apply(BecameFollower @event)
        {
            if (@event.EntityId != this.Id)
            {
                throw new InvalidOperationException("Relationship");
            }
            Relationship current = default(Relationship);
            if (this.Relationships.TryGetValue(@event.RelationshipId, out current))
            {
                throw new InvalidOperationException("Already a follower");

            }

            this.Relationships[@event.RelationshipId] = new Relationship(this.Id, @event.OfUser, RelationshipType.FOLLOWER);
        }


        public void Apply(FriendshipRequested @event)
        {
            if (@event.EntityId != this.Id)
            {
                throw new InvalidOperationException("Relationship");
            }
            Relationship current = default(Relationship);
            if (!this.Relationships.TryGetValue(@event.RelationshipId, out current))
            {
                throw new InvalidOperationException("Tried to request friendship without becoming a follower first");

            }
            if (current.Type != RelationshipType.FOLLOWER)
            {
                throw new InvalidOperationException("Tried to request friendship without becoming a follower first");

            }

            this.Relationships[@event.RelationshipId] = new Relationship(this.Id, current.Target, RelationshipType.FRIEND_REQUEST);
        }

        public void Apply(FriendshipAccepted @event)
        {
            if (@event.EntityId != this.Id)
            {
                throw new InvalidOperationException("Relationship");
            }
            Relationship current = default(Relationship);
            if (!this.Relationships.TryGetValue(@event.RelationshipId, out current))
            {
                throw new InvalidOperationException("Tried to accept friendship without having a request first");

            }
            if (current.Type != RelationshipType.FRIEND_REQUEST)
            {
                throw new InvalidOperationException("Tried to accept friendship without having a request first");

            }

            this.Relationships[@event.RelationshipId] = new Relationship(this.Id, current.Target, RelationshipType.FRIEND);
        }

        public void Apply(AuthTokenSet @event)
        {
            this.AccessToken = @event.AccessToken;
            this.ExpiresIn = @event.ExpiresIn;
            this.RefreshToken = @event.RefreshToken;
        }


        public void Apply(PlantAdded @event)
        {
            if (@event.UserId != this.Id)
            {
                throw new InvalidOperationException("Can't create garden under user with incorrect id");
            }
            GardenState gardenState = null;
            if (this.Gardens.TryGetValue(@event.EntityId, out gardenState))
            {
                gardenState.Apply(@event);
            }
            else
            {
                throw new InvalidOperationException("garden id doesn't exist");
            }
        }

        public void Apply(GardenCreated @event)
        {
            if (@event.UserId != this.Id)
            {
                throw new InvalidOperationException("Can't create garden under user with incorrect id");
            }
            GardenState gardenState = null;
            if (this.Gardens.TryGetValue(@event.EntityId, out gardenState))
            {
                throw new InvalidOperationException("garden already exists, can't recreate");
            }
            else
            {
                this.Gardens[@event.EntityId] = new GardenState(@event);
                if (this.Gardens.Count == 1)
                    this.GardenId = @event.EntityId;
            }
        }

        public void Apply(GardenAdded @event)
        {
            if (@event.EntityId != this.Id)
            {
                throw new InvalidOperationException("Can't create garden under user with incorrect id");
            }
            GardenState gardenState = null;
            if (this.Gardens.TryGetValue(@event.GardenId, out gardenState))
            {
                //this.Gardens[@event.EntityId] = new GardenState(@event);
            }
            else
            {
                throw new InvalidOperationException("garden doesn't exists, can't add");
            }
        }

        public void Apply(ScheduleCreated @event)
        {
            if (@event.UserId != this.Id)
            {
                throw new InvalidOperationException("Can't create schedule under user with incorrect id");
            }
            ScheduleState scheduleState = null;
            if (this.Schedules.TryGetValue(@event.EntityId, out scheduleState))
            {
                throw new InvalidOperationException("PlantAction with this id already exists");
            }
            else
            {
                scheduleState = new ScheduleState(@event);
            }


            this.Schedules[scheduleState.Id] = scheduleState;
        }






    }
}
