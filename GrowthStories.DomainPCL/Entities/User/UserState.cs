using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static readonly Guid UnregUserId = Guid.NewGuid();//new Guid("11000000-0000-0000-0000-000000000011");
        public static readonly Guid UnregUserGardenId = Guid.NewGuid();//new Guid("11100000-0000-0000-0000-000000000111");

        [JsonIgnore]
        public readonly HashSet<Guid> Friends = new HashSet<Guid>();
        [JsonIgnore]
        public readonly HashSet<Guid> Collaborators = new HashSet<Guid>();
        [JsonIgnore]
        public readonly HashSet<Guid> Rejects = new HashSet<Guid>();


        // private IDictionary<Guid, Relationship> OutsiderSourcedRelationships = new Dictionary<Guid, Relationship>();

        [JsonIgnore]
        public IDictionary<Guid, ScheduleState> Schedules { get; private set; }
        [JsonProperty]
        public IDictionary<Guid, GardenState> Gardens { get; private set; }

        [JsonIgnore]
        public GardenState Garden
        {
            get
            {
                try
                {
                    return Gardens.First().Value;
                }
                catch (Exception)
                {

                }
                return null;
            }
        }

        [JsonProperty]
        public string Password { get; private set; }

        [JsonProperty]
        public string Email { get; private set; }

        [JsonProperty]
        public string Username { get; private set; }
 
        [JsonIgnore]
        public string AccessToken { get; set; }

        [JsonIgnore]
        public int ExpiresIn { get; set; }
        
        [JsonIgnore]
        public string RefreshToken { get; set; }

        [JsonProperty]
        public bool IsCollaborator { get; set; }

        public bool IsRegistered { get; set; }

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

        public void Apply(Registered @event)
        {
            this.Username = @event.Username;
            this.IsRegistered = true;

            // these events cannot contain the 
            // username / password information
        }

        /*
        public bool IsRegistered()
        {
            // the UserState is not being updated with email addresses
            // as we don't wish to expose them via the API to followers,
            // therefore we use just the username for this 
            // -- JOJ 4.1.2013

            return Username != null && !Username.Equals(AuthUser.UnregUsername);
            //return Email != null && !Email.StartsWith(AuthUser.UnregEmailPrefix);
        }
        */

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

        public void Apply(UsernameSet @event)
        {
            this.Username = @event.Username;
        }

        public void Apply(PasswordSet @event)
        {
            this.Password = @event.Password;
        }

        public void Apply(EmailSet @event)
        {    
            this.Email = @event.Email;
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
                gardenState = new GardenState(@event);
                this.Gardens[@event.EntityId.Value] = gardenState;
                if (this.Gardens.Count == 1)
                {
                    this.GardenId = gardenState.Id;
                }
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


        public override void Merge(IMessage incoming, IMessage outgoing, out IMessage incomingNew, out IMessage outgoingNew)
        {
            // do nothing by default

            if (incoming is UsernameSet && outgoing is UsernameSet)
            {
                this.MergeByCreated(incoming, outgoing, out incomingNew, out outgoingNew);
            }
            else
            {
                base.Merge(incoming, outgoing, out incomingNew, out outgoingNew);
            }

        }


    }
}
