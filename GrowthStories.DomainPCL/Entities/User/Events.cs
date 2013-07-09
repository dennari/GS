using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using Growthstories.Sync;
using Newtonsoft.Json;


namespace Growthstories.Domain.Messaging
{



    #region User

    [DTOObject(DTOType.createUser)]
    public class UserCreated : EventBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }


        public UserCreated() { }
        public UserCreated(Guid id) : base(id) { }

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

        public Guid OfUser { get; set; }

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

    public class AuthTokenSet : EventBase
    {

        public string AccessToken { get; protected set; }
        public int ExpiresIn { get; protected set; }
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

    public abstract class ActionBase : EventBase, IEntityCommand
    {

        [JsonIgnore]
        public Type EntityType { get { return typeof(User); } }
        [JsonProperty]
        public string Note { get; private set; }
        [JsonProperty]
        public Guid PlantId { get; private set; }

        protected ActionBase() { }

        public ActionBase(Guid userId, Guid plantId, string note)
            : base(userId)
        {
            this.PlantId = plantId;
            this.Note = note;
        }

        public ActionBase(ActionBase other)
            : base(other.EntityId)
        {
            this.PlantId = other.PlantId;
            this.Note = other.Note;
        }

        public abstract override string ToString();


    }

    public class Commented : ActionBase
    {


        protected Commented() { }
        public Commented(Guid userId, Guid plantId, string note)
            : base(userId, plantId, note) { }
        public Commented(Comment cmd)
            : base((ActionBase)cmd) { }

        public override string ToString()
        {
            return string.Format("User {1} commented '{0}' on plant {2}", Note, EntityId, PlantId);
        }


    }

    public class Watered : ActionBase
    {


        protected Watered() { }
        public Watered(Guid userId, Guid plantId, string note)
            : base(userId, plantId, note) { }
        public Watered(Water cmd)
            : base(cmd) { }

        public override string ToString()
        {
            return string.Format("User {1} watered plant {0}", PlantId, EntityId);
        }


    }

    public class Fertilized : ActionBase
    {


        protected Fertilized() { }
        public Fertilized(Guid userId, Guid plantId, string note)
            : base(userId, plantId, note) { }
        public Fertilized(Fertilize cmd)
            : base(cmd) { }

        public override string ToString()
        {
            return string.Format("User {1} Fertilized plant {0}", PlantId, EntityId);
        }


    }

    public class Photographed : ActionBase
    {
        [JsonProperty]
        private string _uri { get; set; }

        private Uri _Uri;

        [JsonIgnore]
        public Uri Uri { get { return _Uri == null ? _Uri = new Uri(_uri) : _Uri; } }

        protected Photographed() { }
        public Photographed(Guid userId, Guid plantId, string note, Uri uri)
            : base(userId, plantId, note)
        {
            this._Uri = uri;
            this._uri = uri.ToString();

        }
        public Photographed(Photograph cmd)
            : this(cmd.EntityId, cmd.PlantId, cmd.Note, cmd.Uri)
        {

        }


        public override string ToString()
        {
            return string.Format("Photographed plant {0} (user {1})", PlantId, EntityId);
        }

    }

    #endregion

}

