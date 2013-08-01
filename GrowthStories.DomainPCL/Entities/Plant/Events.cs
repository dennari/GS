using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using Growthstories.Sync;
using Newtonsoft.Json;


namespace Growthstories.Domain.Messaging
{



    #region Plant

    [DTOObject(DTOType.createPlant)]
    public class PlantCreated : EventBase, ICreateEvent
    {

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string ProfilepicturePath { get; private set; }

        [JsonProperty]
        public Guid UserId { get; private set; }

        [JsonIgnore]
        private Type _AggregateType;
        [JsonIgnore]
        public Type AggregateType
        {
            get { return _AggregateType == null ? _AggregateType = typeof(Plant) : _AggregateType; }
        }


        protected PlantCreated() { }
        public PlantCreated(Guid entityId, string name, Guid userId)
            : base(entityId)
        {
            if (name == null)
            {
                throw new ArgumentNullException("a name has to be provided");
            }
            if (userId == default(Guid))
            {
                throw new ArgumentNullException("userId has to be provided");
            }
            Name = name;
            UserId = userId;
        }

        public PlantCreated(CreatePlant cmd) :
            this(cmd.EntityId, cmd.Name, cmd.UserId)
        {
            this.ProfilepicturePath = cmd.ProfilepicturePath;
        }

        public override string ToString()
        {
            return string.Format(@"Created plant {0}, {1}", Name, EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ICreatePlantDTO)Dto;
            base.FillDTO(D);
            D.Name = this.Name;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ICreatePlantDTO)Dto;
            base.FromDTO(D);
            this.Name = D.Name;
        }



    }

    public class ProfilepicturePathChanged : EventBase
    {

        [JsonProperty]
        public string ProfilepicturePath { get; private set; }

        protected ProfilepicturePathChanged() { }
        public ProfilepicturePathChanged(Guid entityId, string ProfilepicturePath)
            : base(entityId)
        {
            this.ProfilepicturePath = ProfilepicturePath;
        }

        public ProfilepicturePathChanged(ChangeProfilepicturePath cmd)
            : this(cmd.EntityId, cmd.ProfilepicturePath)
        {
        }

        public override string ToString()
        {
            return string.Format(@"ProfilepicturePath changed to {0}", ProfilepicturePath);
        }

    }

    [DTOObject(DTOType.setProperty)]
    public class MarkedPlantPublic : EventBase
    {
        protected MarkedPlantPublic() { }
        public MarkedPlantPublic(Guid entityId) : base(entityId) { }


        public override string ToString()
        {
            return string.Format(@"Marked plant {0} public", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            base.FillDTO(D);
            D.PropertyName = Language.SHARED;
            D.PropertyValue = true;
            D.EntityType = DTOType.plant;

        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            if (D.PropertyName != Language.SHARED)
                throw new ArgumentException();
            if ((bool)D.PropertyValue != true)
                throw new ArgumentException();
            if (D.EntityType != DTOType.plant)
                throw new ArgumentException();
            base.FromDTO(D);

        }



    }

    [DTOObject(DTOType.setProperty)]
    public class MarkedPlantPrivate : EventBase
    {
        public MarkedPlantPrivate() { }
        public MarkedPlantPrivate(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Marked plant {0} private", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            base.FillDTO(D);
            D.PropertyName = Language.SHARED;
            D.PropertyValue = false;
            D.EntityType = DTOType.plant;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            if (D.PropertyName != Language.SHARED)
                throw new ArgumentException();
            if ((bool)D.PropertyValue != false)
                throw new ArgumentException();
            if (D.EntityType != DTOType.plant)
                throw new ArgumentException();
            base.FromDTO(D);

        }

    }



    [DTOObject(DTOType.addFBComment)]
    public class FBCommentAdded : EventBase
    {

        [JsonProperty]
        public String FbId { get; private set; }
        [JsonProperty]
        public long Uid { get; private set; }
        [JsonProperty]
        public String Name { get; private set; }
        [JsonProperty]
        public string FirstName { get; private set; }
        [JsonProperty]
        public String LastName { get; private set; }
        [JsonProperty]
        public String Note { get; private set; }

        protected FBCommentAdded() { }
        public FBCommentAdded(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Added water to plant {0}", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IAddFBCommentDTO)Dto;
            D.FbId = this.FbId;
            D.Uid = this.Uid;
            D.Name = this.Name;
            D.FirstName = this.FirstName;
            D.LastName = this.LastName;
            D.Note = this.Note;
            base.FillDTO(D);
            //D.
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IAddFBCommentDTO)Dto;
            this.FbId = D.FbId;
            this.Uid = D.Uid;
            this.Name = D.Name;
            this.FirstName = D.FirstName;
            this.LastName = D.LastName;
            this.Note = D.Note;
            base.FromDTO(D);

        }
    }



    #endregion


}

