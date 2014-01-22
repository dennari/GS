using System;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Sync;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Growthstories.Domain.Messaging
{


    #region User

    [DTOObject(DTOType.register)]
    public sealed class Registered : EventBase, IAggregateEvent<UserState>
    {

        [JsonProperty]
        public string Username { get; private set; }
        [JsonProperty]
        public string Password { get; private set; }
        [JsonProperty]
        public string Email { get; private set; }

        private Registered() { }

        public override bool FillDTO(IEventDTO Dto)
        {
            // no need for translation to work in this direction
            throw new NotImplementedException();
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as IRegisterDTO;
            if (D == null)
                return false;

            this.Username = D.Username;
            this.Password = D.Password;
            this.Email = D.Email;

            return base.FromDTO(D);
        }

        public UserState AggregateState { get; set; }

    }

    [DTOObject(DTOType.createUser)]
    public sealed class UserCreated : EventBase, ICreateMessage, IAggregateEvent<UserState>
    {
        [JsonProperty]
        public string Username { get; private set; }
        [JsonProperty]
        public string Password { get; private set; }
        [JsonProperty]
        public string Email { get; private set; }

        [JsonIgnore]
        private Type _AggregateType;
        [JsonIgnore]
        public Type AggregateType
        {
            get { return _AggregateType == null ? _AggregateType = typeof(User) : _AggregateType; }
        }

        private UserCreated() { }
        public UserCreated(Guid id, string username, string password, string email)
            : base(id)
        {
            if (username == null || password == null || email == null)
                throw new ArgumentNullException();
            this.Username = username;
            this.Password = password;
            this.Email = email;
            this.StreamType = DTOType.user;
            this.StreamEntityId = id;
        }
        public UserCreated(CreateUser cmd)
            : base(cmd)
        {
            if (cmd.Username == null || cmd.Password == null || cmd.Email == null)
                throw new ArgumentNullException();
            this.Username = cmd.Username;
            this.Password = cmd.Password;
            this.Email = cmd.Email;

            //this.StreamEntityId = cmd.
        }

        public override string ToString()
        {
            return string.Format(@"Created user {0}", AggregateId);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ICreateUserDTO; if (Dto == null) return false;
            D.Username = this.Username;
            D.Password = this.Password;
            D.Email = this.Email;
            D.StreamAncestor = null;
            D.AncestorId = null;
            return base.FillDTO(D);
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ICreateUserDTO;
            if (D == null)
                return false;

            this.Username = D.Username;
            this.Password = D.Password;
            this.Email = D.Email;
            return base.FromDTO(D);
        }


        public UserState AggregateState { get; set; }

    }

    [DTOObject(DTOType.setProperty)]
    public sealed class UsernameSet : EventBase
    {
        [JsonProperty]
        public string Username { get; private set; }


        private UsernameSet() { }
        public UsernameSet(SetUsername cmd)
            : base(cmd)
        {
            if (cmd.Username == null)
                throw new ArgumentNullException();
            this.Username = cmd.Username;

        }

        public override string ToString()
        {
            return string.Format(@"User name set to {0}", Username);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            D.EntityType = DTOType.user;
            D.PropertyName = "username";
            D.PropertyValue = this.Username;
            return base.FillDTO(D);
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO;
            if (D == null)
                return false;

            if (D.EntityType != DTOType.user || D.PropertyName != "username")
                return false;
            this.Username = (string)D.PropertyValue;
            return base.FromDTO(D);

        }

    }

    [DTOObject(DTOType.setProperty)]
    public sealed class EmailSet : EventBase
    {
        [JsonProperty]
        public string Email { get; private set; }


        private EmailSet() { }
        public EmailSet(SetEmail cmd)
            : base(cmd)
        {
            if (cmd.Email == null)
                throw new ArgumentNullException();
            this.Email = cmd.Email;

        }

        public override string ToString()
        {
            return string.Format(@"Email set to {0}", Email);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            D.EntityType = DTOType.user;
            D.PropertyName = "email";
            D.PropertyValue = this.Email;
            return base.FillDTO(D);
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            if (D.EntityType != DTOType.user || D.PropertyName != "email")
                return false;
            this.Email = (string)D.PropertyValue;
            return base.FromDTO(D);

        }

    }


    [DTOObject(DTOType.setProperty)]
    public sealed class PasswordSet : EventBase
    {
        [JsonProperty]
        public string Password { get; private set; }


        private PasswordSet() { }
        public PasswordSet(SetPassword cmd)
            : base(cmd)
        {
            if (cmd.Password == null)
                throw new ArgumentNullException();
            this.Password = cmd.Password;

        }

        public override string ToString()
        {
            return string.Format(@"Password set to {0}", Password);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            D.EntityType = DTOType.user;
            D.PropertyName = "password";
            D.PropertyValue = this.Password;
            return base.FillDTO(D);
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            if (D.EntityType != DTOType.user || D.PropertyName != "password")
                return false;
            this.Password = (string)D.PropertyValue;
            return base.FromDTO(D);

        }

    }



    public abstract class RelationshipEvent : EventBase
    {

        [JsonProperty]
        public Guid Target { get; private set; }

        protected RelationshipEvent() { }
        public RelationshipEvent(RelationshipCommand cmd)
            : base(cmd)
        {
            this.Target = cmd.Target;
        }


        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;

            D.EntityType = DTOType.user;
            D.PropertyValue = new JObject();
            D.PropertyValue[Language.PROPERTY_ANCESTOR_ID] = null;
            D.PropertyValue[Language.PROPERTY_ENTITY_ID] = this.Target.ToString();


            D.StreamAncestor = null;
            D.AncestorId = null;

            return base.FillDTO(D);

        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            base.FromDTO(D);

            try
            {
                var val = (JObject)D.PropertyValue;
                this.Target = Guid.Parse(val[Language.PROPERTY_ENTITY_ID].ToString());
                return true;
                //this.AggregateId = Guid.Parse(val[Language.PROPERTY_ANCESTOR_ID].ToString());
            }
            catch
            {
                return false;
            }

        }
    }


    [DTOObject(DTOType.setProperty)]
    public sealed class BecameFollower : RelationshipEvent
    {


        private BecameFollower() { }

        public BecameFollower(BecomeFollower cmd)
            : base(cmd)
        {
        }

        public override string ToString()
        {
            return string.Format(@"User {0} became a follower of user {1}.", this.AggregateId, this.Target);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            D.PropertyName = "following";
            return base.FillDTO(D);
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            if (D.EntityType != DTOType.user || D.PropertyName != "following")
                return false;

            return base.FromDTO(D);

        }

    }

    [DTOObject(DTOType.delProperty)]
    public sealed class UnFollowed : RelationshipEvent
    {


        private UnFollowed() { }

        public UnFollowed(UnFollow cmd)
            : base(cmd)
        {
        }

        public override string ToString()
        {
            return string.Format(@"User {0} stopped following user {1}.", this.AggregateId, this.Target);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as IDelPropertyDTO; if (Dto == null) return false;
            D.PropertyName = "following";
            return base.FillDTO(D);
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as IDelPropertyDTO; if (Dto == null) return false;
            if (D.EntityType != DTOType.user || D.PropertyName != "following")
                return false;

            return base.FromDTO(D);

        }

    }

    [DTOObject(DTOType.setProperty)]
    public sealed class CollaborationRequested : RelationshipEvent
    {


        private CollaborationRequested() { }
        public CollaborationRequested(RequestCollaboration cmd)
            : base(cmd)
        {
        }

        public override string ToString()
        {
            return string.Format(@"User {0}: requested collaboration with {1}.", this.AggregateId, this.Target);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            D.PropertyName = "wannabes";
            return base.FillDTO(D);
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            if (D.EntityType != DTOType.user || D.PropertyName != "wannabes")
                return false;

            return base.FromDTO(D);

        }

    }


    [DTOObject(DTOType.setProperty)]
    public sealed class CollaborationDenied : RelationshipEvent
    {


        private CollaborationDenied() { }
        public CollaborationDenied(DenyCollaboration cmd)
            : base(cmd)
        {
        }

        public override string ToString()
        {
            return string.Format(@"User {0}: denied collaboration with {1}.", this.AggregateId, this.Target);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            D.PropertyName = "unworthies";
            return base.FillDTO(D);
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            if (D.EntityType != DTOType.user || D.PropertyName != "unworthies")
                return false;

            return base.FromDTO(D);
        }

    }



    [DTOObject(DTOType.setProperty)]
    public sealed class GardenAdded : EventBase, IAggregateEvent<UserState>
    {
        [JsonProperty]
        public Guid GardenId { get; private set; }



        private GardenAdded() { }


        public GardenAdded(AddGarden cmd)
            : base(cmd)
        {
            this.GardenId = cmd.GardenId;
        }

        public override string ToString()
        {
            return string.Format(@"User {0} added garden {1}.", this.AggregateId, this.GardenId);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            D.EntityType = DTOType.user;
            D.PropertyName = "garden";
            D.PropertyValue = new JObject();

            D.PropertyValue[Language.PROPERTY_ANCESTOR_ID] = this.AggregateId.ToString();
            D.PropertyValue[Language.PROPERTY_ENTITY_ID] = this.GardenId.ToString();


            D.StreamAncestor = null;
            D.AncestorId = null;

            return base.FillDTO(D);


        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            if (D.EntityType != DTOType.user || D.PropertyName != "garden")
                return false;

            base.FromDTO(D);

            try
            {
                var val = (JObject)D.PropertyValue;
                this.GardenId = Guid.Parse(val[Language.PROPERTY_ENTITY_ID].ToString());
                //this.AggregateId = Guid.Parse(val[Language.PROPERTY_ANCESTOR_ID].ToString());
                return true;
            }
            catch
            {
                return false;
            }

        }


        public UserState AggregateState { get; set; }
    }



    [DTOObject(DTOType.setProperty)]
    public sealed class LocationEnabledSet : EventBase, IAggregateEvent<UserState>
    {

        [JsonProperty]
        public bool LocationEnabled { get; private set; }

        private LocationEnabledSet() { }

        public LocationEnabledSet(SetLocationEnabled cmd)
            : base(cmd)
        {
            LocationEnabled = cmd.LocationEnabled;
        }

        public override string ToString()
        {
            return string.Format(@"User {0} set location enabled to {1}.", this.AggregateId, LocationEnabled);
        }


        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            D.EntityType = DTOType.user;
            D.PropertyName = "locationEnabled";
            D.PropertyValue = LocationEnabled;

            D.StreamAncestor = null;
            D.AncestorId = null;
            return base.FillDTO(D);
        }


        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            if (D.EntityType != DTOType.user || D.PropertyName != "locationEnabled")
                return false;

            this.LocationEnabled = D.PropertyValue;
            return base.FromDTO(D);
        }


        public UserState AggregateState { get; set; }

    }






    #endregion



}

