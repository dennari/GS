using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using Growthstories.Sync;
using Newtonsoft.Json;


namespace Growthstories.Domain.Messaging
{



    #region User

    [DTOObject(DTOType.createUser)]
    public class UserCreated : EventBase, ICreateEvent
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

        protected UserCreated() { }
        public UserCreated(Guid id, string username, string password, string email)
            : base(id)
        {
            if (username == null || password == null || email == null)
                throw new ArgumentNullException();
            this.Username = username;
            this.Password = password;
            this.Email = email;
            this.HasAncestor = false;
            this.HasParent = false;
        }
        public UserCreated(CreateUser cmd)
            : this(cmd.EntityId, cmd.Username, cmd.Password, cmd.Email)
        {
        }

        public override string ToString()
        {
            return string.Format(@"Created user {0}", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ICreateUserDTO)Dto;
            base.FillDTO(D);
            D.Username = this.Username;
            D.Password = this.Password;
            D.Email = this.Email;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ICreateUserDTO)Dto;
            base.FromDTO(D);
            this.Username = D.Username;
            this.Password = D.Password;
            this.Email = D.Email;
        }

    }


    [DTOObject(DTOType.addRelationship)]
    public class BecameFollower : EventBase
    {

        [JsonProperty]
        public Guid OfUser { get; private set; }

        protected BecameFollower() { }
        public BecameFollower(Guid id, Guid OfUser)
            : base(id)
        {
            this.OfUser = OfUser;
        }

        public BecameFollower(BecomeFollower command)
            : this(command.EntityId, command.OfUser)
        {

        }

        public override string ToString()
        {
            return string.Format(@"User {0} wants to become a follower of user {1}.", this.EntityId, this.OfUser);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IAddRelationshipDTO)Dto;
            base.FillDTO(D);
            D.To = this.OfUser;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IAddRelationshipDTO)Dto;
            base.FromDTO(D);
            this.OfUser = D.To;
        }

    }


    public class GardenAdded : EventBase
    {

        [JsonProperty]
        public Guid GardenId { get; private set; }

        protected GardenAdded() { }
        public GardenAdded(Guid id, Guid gardenId)
            : base(id)
        {
            this.GardenId = gardenId;
        }

        public GardenAdded(AddGarden command)
            : this(command.EntityId, command.GardenId)
        {

        }

        public override string ToString()
        {
            return string.Format(@"User {0} added garden {1}.", this.EntityId, this.GardenId);
        }


    }


    public class AuthTokenSet : EventBase
    {

        [JsonProperty]
        public string AccessToken { get; protected set; }
        [JsonProperty]
        public int ExpiresIn { get; protected set; }
        [JsonProperty]
        public string RefreshToken { get; protected set; }

        protected AuthTokenSet() { }

        public AuthTokenSet(SetAuthToken cmd) : this(cmd.EntityId, cmd.AccessToken, cmd.RefreshToken, cmd.ExpiresIn) { }

        public AuthTokenSet(Guid id, string accessToken, string refreshToken, int expiresIn)
            : base(id)
        {
            this.AccessToken = accessToken;
            this.ExpiresIn = expiresIn;
            this.RefreshToken = refreshToken;
        }

        public override string ToString()
        {
            return string.Format(@"AuthTokenSet access: {0}, refresh: {1}, expires {2}, for user {3}.", AccessToken, RefreshToken, ExpiresIn, EntityId);
        }

    }



    #endregion



}

