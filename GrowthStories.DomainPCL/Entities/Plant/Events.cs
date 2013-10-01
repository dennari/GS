using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using Growthstories.Sync;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace Growthstories.Domain.Messaging
{



    #region Plant

    [DTOObject(DTOType.createPlant)]
    public class PlantCreated : EventBase, ICreateEvent
    {

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string Species { get; private set; }

        [JsonProperty]
        public Photo Profilepicture { get; private set; }

        [JsonProperty]
        public Guid UserId { get; private set; }

        [JsonProperty]
        public Guid FertilizingScheduleId { get; private set; }

        [JsonProperty]
        public Guid WateringScheduleId { get; private set; }

        [JsonProperty]
        public HashSet<string> Tags { get; private set; }

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
            base(cmd)
        {

            if (cmd.Name == null)
            {
                throw new ArgumentNullException("a name has to be provided");
            }
            if (cmd.UserId == default(Guid))
            {
                throw new ArgumentNullException("userId has to be provided");
            }
            this.Name = cmd.Name;
            this.UserId = cmd.UserId;
            this.Profilepicture = cmd.Profilepicture;
            this.Species = cmd.Species;
            this.WateringScheduleId = cmd.WateringScheduleId;
            this.FertilizingScheduleId = cmd.FertilizingScheduleId;
            this.Tags = cmd.Tags;




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

    public class ProfilepictureSet : EventBase
    {

        [JsonProperty]
        public Photo Profilepicture { get; private set; }

        protected ProfilepictureSet() { }
        public ProfilepictureSet(Guid entityId, Photo profilepicture)
            : base(entityId)
        {
            this.Profilepicture = profilepicture;
        }

        public ProfilepictureSet(SetProfilepicture cmd)
            : base(cmd)
        {
            this.Profilepicture = cmd.Profilepicture;

        }

        public override string ToString()
        {
            return string.Format(@"ProfilepicturePath changed to {0}", Profilepicture);
        }

    }

    [DTOObject(DTOType.setProperty)]
    public class MarkedPlantPublic : EventBase
    {
        protected MarkedPlantPublic() { }
        public MarkedPlantPublic(Guid entityId) : base(entityId) { }
        public MarkedPlantPublic(MarkPlantPublic cmd) : base(cmd) { }


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
        public MarkedPlantPrivate(MarkPlantPrivate cmd) : base(cmd) { }


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

    [DTOObject(DTOType.setProperty)]
    public class WateringScheduleSet : EventBase
    {

        public Guid ScheduleId { get; protected set; }

        protected WateringScheduleSet() { }
        //public WateringScheduleSet(Guid plantId, Guid scheduleId)
        //    : base(plantId)
        //{
        //    this.ScheduleId = ScheduleId;
        //}

        public WateringScheduleSet(SetWateringSchedule cmd, Guid userId)
            : base(cmd)
        {
            this.ScheduleId = cmd.ScheduleId;
            this.StreamEntityId = cmd.EntityId;
            this.StreamAncestorId = userId;
            this.AncestorId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"WateringSchedule set to {1} for plant {0}.", EntityId, ScheduleId);
        }
        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            D.EntityType = DTOType.plant;
            D.PropertyName = "wateringSchedule";
            D.PropertyValue = new JObject();
            D.PropertyValue[Language.PROPERTY_ANCESTOR_ID] = this.AncestorId.ToString();
            D.PropertyValue[Language.PROPERTY_ENTITY_ID] = this.ScheduleId.ToString();

            base.FillDTO(D);

        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;


            base.FromDTO(D);
        }

    }

    [DTOObject(DTOType.setProperty)]
    public class FertilizingScheduleSet : EventBase
    {

        public Guid ScheduleId { get; protected set; }

        protected FertilizingScheduleSet() { }
        //public FertilizingScheduleSet(Guid entityId, Guid scheduleId)
        //    : base(entityId)
        //{
        //    this.ScheduleId = ScheduleId;
        //}

        public FertilizingScheduleSet(SetFertilizingSchedule cmd, Guid userId)
            : base(cmd)
        {
            this.ScheduleId = cmd.ScheduleId;
            this.StreamEntityId = cmd.EntityId;
            this.StreamAncestorId = userId;
            this.AncestorId = userId;
        }


        public override string ToString()
        {
            return string.Format(@"Schedule set to {1} for plant {0}.", EntityId, ScheduleId);
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

    public class TagsSet : EventBase
    {
        [JsonProperty]
        public HashSet<string> Tags { get; private set; }

        protected TagsSet() { }
        public TagsSet(Guid plantId, HashSet<string> tags)
            : base(plantId)
        {
            this.Tags = tags;
        }

        public TagsSet(SetTags cmd)
            : base(cmd)
        {
            this.Tags = cmd.Tags;
        }


        public override string ToString()
        {
            return string.Format(@"Tags set for plant {0}.", EntityId);
        }

    }

    public class NameSet : EventBase
    {
        [JsonProperty]
        public string Name { get; private set; }

        protected NameSet() { }
        public NameSet(Guid plantId, string name)
            : base(plantId)
        {
            this.Name = name;
        }

        public NameSet(SetName cmd)
            : base(cmd)
        {
            this.Name = cmd.Name;
        }

        public override string ToString()
        {
            return string.Format(@"Name set to {1} for plant {0}.", EntityId, Name);
        }

    }

    public class SpeciesSet : EventBase
    {
        [JsonProperty]
        public string Species { get; private set; }

        protected SpeciesSet() { }
        public SpeciesSet(Guid plantId, string species)
            : base(plantId)
        {
            this.Species = species;
        }

        public SpeciesSet(SetSpecies cmd)
            : base(cmd)
        {
            this.Species = cmd.Species;
        }

        public override string ToString()
        {
            return string.Format(@"Species set to {1} for plant {0}.", EntityId, Species);
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

