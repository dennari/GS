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

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IActionDTO)Dto;
            D.Note = this.Note;
            D.PlantId = this.PlantId;
            base.FillDTO(D);
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IActionDTO)Dto;
            this.Note = D.Note;
            this.PlantId = D.PlantId;
            base.FromDTO(D);
        }


    }

    [DTOObject(DTOType.addComment)]
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

    public class Measured : ActionBase
    {

        [JsonProperty]
        public MeasurementType Series { get; set; }
        [JsonProperty]
        public double Value { get; set; }

        protected Measured() { }
        public Measured(Guid userId, Guid plantId, string note, MeasurementType series, double value)
            : base(userId, plantId, note)
        {
            this.Series = series;
            this.Value = value;
        }
        public Measured(Measure cmd)
            : this(cmd.EntityId, cmd.PlantId, cmd.Note, cmd.Series, cmd.Value)
        {

        }

        public override string ToString()
        {
            return string.Format("User {1} Measured plant {0}: {2}, {3}", PlantId, EntityId, Series.ToString(), Value);
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
            if (uri == null)
                throw new ArgumentNullException("Uri for the image file must be given for the 'Photographed' action.");
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

    public enum MeasurementType
    {
        PH,
        SOIL_HUMIDITY,
        AIR_HUMIDITY,
        LENGTH,
        WEIGHT,
        ILLUMINANCE
    }

}

