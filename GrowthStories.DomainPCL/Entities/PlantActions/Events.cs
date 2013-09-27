using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{



    #region PlantAction

    [DTOObject(DTOType.addComment, DTOType.addWatering, DTOType.addPhoto, DTOType.addFertilizing, DTOType.addMeasurement)]
    public class PlantActionCreated : EventBase, ICreateEvent
    {
        [JsonIgnore]
        private Type _AggregateType;
        [JsonIgnore]
        public Type AggregateType
        {
            get { return _AggregateType == null ? _AggregateType = typeof(PlantAction) : _AggregateType; }
        }

        [JsonProperty]
        public PlantActionType Type { get; private set; }

        [JsonProperty]
        public Guid UserId { get; private set; }

        [JsonProperty]
        public Guid PlantId { get; private set; }

        [JsonProperty]
        public string Note { get; private set; }

        [JsonProperty]
        public MeasurementType MeasurementType { get; set; }

        [JsonProperty]
        public double Value { get; set; }

        [JsonProperty]
        public Photo Photo { get; set; }

        protected PlantActionCreated() { }
        public PlantActionCreated(Guid id, Guid userId, Guid plantId, PlantActionType type, string note)
            : base(id)
        {
            this.UserId = userId;
            this.PlantId = plantId;
            this.Type = type;
            this.Note = note;
        }

        public PlantActionCreated(CreatePlantAction cmd)
            : this(cmd.EntityId, cmd.UserId, cmd.PlantId, cmd.Type, cmd.Note)
        {
            this.MeasurementType = cmd.MeasurementType;
            this.Value = cmd.Value;
            this.Photo = cmd.Photo;
        }

        public override string ToString()
        {
            return string.Format(@"Created PlantAction {0}", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ICreatePlantActionDTO)Dto;
            D.Note = this.Note;
            D.ParentId = this.PlantId;

            if (this.Type == PlantActionType.COMMENTED)
                D.EventType = DTOType.addComment;
            if (this.Type == PlantActionType.FERTILIZED)
                D.EventType = DTOType.addFertilizing;
            if (this.Type == PlantActionType.WATERED)
                D.EventType = DTOType.addWatering;
            if (this.Type == PlantActionType.MEASURED)
            {
                D.EventType = DTOType.addMeasurement;
                D.MeasurementType = this.MeasurementType;
                D.Value = this.Value;
            }
            if (this.Type == PlantActionType.PHOTOGRAPHED)
            {
                D.EventType = DTOType.addPhoto;
                D.BlobKey = this.Photo.BlobKey;
            }




            base.FillDTO(D);
            //D.Name = this.Name;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ICreatePlantActionDTO)Dto;

            this.Note = D.Note;

            if (D.EventType == DTOType.addComment)
                this.Type = PlantActionType.COMMENTED;
            if (D.EventType == DTOType.addFertilizing)
                this.Type = PlantActionType.FERTILIZED;
            if (D.EventType == DTOType.addWatering)
                this.Type = PlantActionType.WATERED;
            if (D.EventType == DTOType.addMeasurement)
            {
                this.Type = PlantActionType.MEASURED;
                this.MeasurementType = D.MeasurementType;
                this.Value = D.Value;
            }
            if (D.EventType == DTOType.addPhoto)
            {
                this.Type = PlantActionType.PHOTOGRAPHED;
                var p = this.Photo;
                p.BlobKey = D.BlobKey;
                this.Photo = p;
            }

            base.FromDTO(D);
        }


    }

    public class PlantActionPropertySet : EventBase
    {

        [JsonProperty]
        public PlantActionType Type { get; private set; }

        [JsonProperty]
        public string Note { get; set; }

        [JsonProperty]
        public MeasurementType MeasurementType { get; set; }

        [JsonProperty]
        public double Value { get; set; }

        [JsonProperty]
        public Photo Photo { get; set; }

        protected PlantActionPropertySet() { }
        public PlantActionPropertySet(Guid id, PlantActionType type)
            : base(id)
        {
            this.Type = type;
        }

        public PlantActionPropertySet(SetPlantActionProperty cmd)
            : this(cmd.EntityId, cmd.Type)
        {
            this.Note = cmd.Note;
            this.MeasurementType = cmd.MeasurementType;
            this.Value = cmd.Value;
            this.Photo = cmd.Photo;
        }
    }
    #endregion


}

