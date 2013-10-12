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
        WANNABE,
        UNWORTHY
    }

    public sealed class Relationship
    {
        public readonly Guid Source;
        public readonly Guid Target;
        public RelationshipType Type;
        public Relationship(Guid source, Guid target, RelationshipType type)
        {
            this.Source = source;
            this.Target = target;
            this.Type = type;
        }
    }

    public class UserState : AggregateState<UserCreated>, IAuthUser
    {

        public static readonly Guid UnregUserId = new Guid("11000000-0000-0000-0000-000000000011");
        public static readonly Guid UnregUserGardenId = new Guid("11100000-0000-0000-0000-000000000111");

        private HashSet<Guid> Friends = new HashSet<Guid>();

        private HashSet<Guid> Collaborators = new HashSet<Guid>();

        private HashSet<Guid> Rejects = new HashSet<Guid>();


        // private IDictionary<Guid, Relationship> OutsiderSourcedRelationships = new Dictionary<Guid, Relationship>();


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

            this.Friends.Add(@event.Target);

        }


        public void Apply(CollaborationRequested @event)
        {
            this.Collaborators.Add(@event.Target);
        }

        public void Apply(CollaborationDenied @event)
        {
            this.Rejects.Add(@event.Target);
        }


        public void Apply(PlantAdded @event)
        {
            if (@event.AggregateId != this.Id)
            {
                throw new InvalidOperationException("Can't create garden under user with incorrect id");
            }
            GardenState gardenState = null;
            if (this.Gardens.TryGetValue(@event.EntityId.Value, out gardenState))
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
            if (@event.AggregateId != this.Id)
            {
                throw new InvalidOperationException("Can't create garden under user with incorrect id");
            }
            GardenState gardenState = null;
            if (this.Gardens.TryGetValue(@event.EntityId.Value, out gardenState))
            {
                throw new InvalidOperationException("garden already exists, can't recreate");
            }
            else
            {
                this.Gardens[@event.EntityId.Value] = new GardenState(@event);
                if (this.Gardens.Count == 1)
                    this.GardenId = @event.EntityId.Value;
            }
        }

        public void Apply(GardenAdded @event)
        {
            if (@event.AggregateId != this.Id)
            {
                throw new InvalidOperationException("Can't create garden under user with incorrect id");
            }

        }

        public void Apply(ScheduleCreated @event)
        {
            if (@event.AggregateId != this.Id)
            {
                throw new InvalidOperationException("Can't create schedule under user with incorrect id");
            }
            ScheduleState scheduleState = null;
            if (this.Schedules.TryGetValue(@event.EntityId.Value, out scheduleState))
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
