using System;
using System.Collections.Generic;
using System.Linq;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Sync;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Growthstories.Domain.Messaging
{



    #region PlantAction

    [DTOObject(DTOType.addComment,
        DTOType.addWatering,
        DTOType.addPhoto,
        DTOType.addFertilizing,
        DTOType.addMeasurement,
        DTOType.addFBComment,
        DTOType.addBlooming,
        DTOType.addDeceased,
        DTOType.addHarvesting,
        DTOType.addMisting,
        DTOType.addPollination,
        DTOType.addPruning,
        DTOType.addSprouting,
        DTOType.addTransfer

        )]
    public class PlantActionCreated : EventBase, ICreateMessage, IAggregateEvent<PlantActionState>
    {
        [JsonIgnore]
        private Type _AggregateType;
        [JsonIgnore]
        public Type AggregateType
        {
            get { return _AggregateType ?? (_AggregateType = typeof(PlantAction)); }
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
        public double? Value { get; set; }

        [JsonProperty]
        public Photo Photo { get; set; }

        #region FBComment
        [JsonProperty]
        public long FBUid { get; set; }

        [JsonProperty]
        public string FBName { get; set; }
        #endregion



        protected PlantActionCreated() { }
        //public PlantActionCreated(Guid id, Guid userId, Guid plantId, PlantActionType type, string note)
        //    : base(id)
        //{
        //    this.UserId = userId;
        //    this.PlantId = plantId;
        //    this.Type = type;
        //    this.Note = note;
        //}

        public PlantActionCreated(CreatePlantAction cmd)
            : base(cmd)
        {

            if (cmd.UserId == default(Guid))
            {
                throw new ArgumentNullException("UserId has to be provided");
            }
            if (cmd.PlantId == default(Guid))
            {
                throw new ArgumentNullException("PlantId has to be provided");
            }
            if (cmd.Type == PlantActionType.NOTYPE)
            {
                throw new ArgumentNullException("PlantActionType has to be provided");
            }
            if (cmd.Type == PlantActionType.PHOTOGRAPHED && cmd.Photo == null)
            {
                throw new ArgumentNullException("PhotoAction needs photo");
            }
            if (cmd.Type == PlantActionType.MEASURED && (cmd.MeasurementType == Sync.MeasurementType.NOTYPE || !cmd.Value.HasValue))
            {
                throw new ArgumentNullException("MeasurementAction needs measurementType and value");
            }
            if (cmd.Type == PlantActionType.FBCOMMENTED &&
                (string.IsNullOrWhiteSpace(cmd.FBName) ||
                cmd.FBUid == default(long)))
            {
                throw new ArgumentNullException("FBComment needs all four required properties.");
            }

            this.MeasurementType = cmd.MeasurementType;
            this.Value = cmd.Value;
            this.Photo = cmd.Photo;
            this.UserId = cmd.UserId;
            this.PlantId = cmd.PlantId;
            this.Type = cmd.Type;
            this.Note = cmd.Note;
            this.FBName = cmd.FBName;
            this.FBUid = cmd.FBUid;

            //this.AncestorId = userId;
            //this.ParentId = plantId;
            //this.ParentAncestorId = userId;
            //this.StreamAncestorId = userId;
            //this.StreamEntityId = plantId;
        }

        public override string ToString()
        {
            return string.Format(@"Created PlantAction of type {0}", Type);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ICreatePlantActionDTO; if (Dto == null) return false;

            base.FillDTO(D);

            D.Note = this.Note;
            D.ParentId = this.PlantId;

            if (this.Type == PlantActionType.COMMENTED)
                D.EventType = DTOType.addComment;
            if (this.Type == PlantActionType.FERTILIZED)
                D.EventType = DTOType.addFertilizing;
            if (this.Type == PlantActionType.WATERED)
                D.EventType = DTOType.addWatering;
            if (this.Type == PlantActionType.BLOOMED)
                D.EventType = DTOType.addBlooming;
            if (this.Type == PlantActionType.DECEASED)
                D.EventType = DTOType.addDeceased;
            if (this.Type == PlantActionType.HARVESTED)
                D.EventType = DTOType.addHarvesting;
            if (this.Type == PlantActionType.MISTED)
                D.EventType = DTOType.addMisting;
            if (this.Type == PlantActionType.POLLINATED)
                D.EventType = DTOType.addPollination;
            if (this.Type == PlantActionType.PRUNED)
                D.EventType = DTOType.addPruning;
            if (this.Type == PlantActionType.SPROUTED)
                D.EventType = DTOType.addSprouting;
            if (this.Type == PlantActionType.TRANSFERRED)
                D.EventType = DTOType.addTransfer;

            if (this.Type == PlantActionType.MEASURED)
            {
                D.EventType = DTOType.addMeasurement;
                D.MeasurementType = this.MeasurementType;
                D.Value = this.Value.Value;
            }
            if (this.Type == PlantActionType.PHOTOGRAPHED)
            {
                D.EventType = DTOType.addPhoto;
                D.BlobKey = this.Photo.BlobKey;
            }
            if (this.Type == PlantActionType.FBCOMMENTED)
            {
                D.EventType = DTOType.addFBComment;
                D.FBName = this.FBName;
                D.FBUid = this.FBUid;
            }

            return true;
            //D.Name = this.Name;
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ICreatePlantActionDTO; if (Dto == null) return false;

            this.Note = D.Note;

            if (D.EventType == DTOType.addComment)
                this.Type = PlantActionType.COMMENTED;
            if (D.EventType == DTOType.addFertilizing)
                this.Type = PlantActionType.FERTILIZED;
            if (D.EventType == DTOType.addWatering)
                this.Type = PlantActionType.WATERED;
            if (D.EventType == DTOType.addBlooming)
                this.Type = PlantActionType.BLOOMED;
            if (D.EventType == DTOType.addDeceased)
                this.Type = PlantActionType.DECEASED;
            if (D.EventType == DTOType.addHarvesting)
                this.Type = PlantActionType.HARVESTED;
            if (D.EventType == DTOType.addMisting)
                this.Type = PlantActionType.MISTED;
            if (D.EventType == DTOType.addPollination)
                this.Type = PlantActionType.POLLINATED;
            if (D.EventType == DTOType.addPruning)
                this.Type = PlantActionType.PRUNED;
            if (D.EventType == DTOType.addSprouting)
                this.Type = PlantActionType.SPROUTED;
            if (D.EventType == DTOType.addTransfer)
                this.Type = PlantActionType.TRANSFERRED;

            if (D.EventType == DTOType.addMeasurement)
            {
                this.Type = PlantActionType.MEASURED;
                this.MeasurementType = D.MeasurementType;
                this.Value = D.Value;
            }
            if (D.EventType == DTOType.addPhoto)
            {
                this.Type = PlantActionType.PHOTOGRAPHED;
                this.Photo = new Photo()
                {
                    BlobKey = D.BlobKey
                };
            }
            if (D.EventType == DTOType.addFBComment)
            {
                this.Type = PlantActionType.FBCOMMENTED;
                this.FBName = D.FBName;
                this.FBUid = D.FBUid;
            }

            base.FromDTO(D);
            if (this.AncestorId.HasValue)
                this.UserId = this.AncestorId.Value;
            if (this.ParentId.HasValue)
                this.PlantId = this.ParentId.Value;

            return true;

        }



        public PlantActionState AggregateState { get; set; }

    }



    [DTOObject(DTOType.setProperty)]
    public class PlantActionPropertySet : EventBase
    {

        [JsonProperty]
        public PlantActionType Type { get; private set; }

        [JsonProperty]
        public string Note { get; set; }

        [JsonProperty]
        public MeasurementType MeasurementType { get; set; }

        [JsonProperty]
        public double? Value { get; set; }

        [JsonProperty]
        public Photo Photo { get; set; }

        protected PlantActionPropertySet() { }
        //public PlantActionPropertySet(Guid id, PlantActionType type)
        //    : base(id)
        //{
        //    this.Type = type;
        //}

        public PlantActionPropertySet(SetPlantActionProperty cmd, PlantActionType type)
            : base(cmd)
        {


            if (type == PlantActionType.NOTYPE)
            {
                throw new ArgumentNullException("PlantActionType has to be provided");
            }
            //if (type == PlantActionType.PHOTOGRAPHED && cmd.Photo == null)
            //{
            //    throw new ArgumentNullException("PhotoAction needs photo");
            //}
            //if (type == PlantActionType.MEASURED && (cmd.MeasurementType == Sync.MeasurementType.NOTYPE || !cmd.Value.HasValue))
            //{
            //    throw new ArgumentNullException("MeasurementAction needs measurementType and value");
            //}


            this.Type = type;
            this.Note = cmd.Note;
            this.MeasurementType = cmd.MeasurementType;
            this.Value = cmd.Value;
            this.Photo = cmd.Photo;


        }

        public static Dictionary<DTOType, PlantActionType> _ValidTypes;
        public static Dictionary<DTOType, PlantActionType> ValidTypes
        {
            get
            {
                if (_ValidTypes == null)
                {
                    _ValidTypes = new Dictionary<DTOType, PlantActionType>() { 
                {DTOType.comment, PlantActionType.COMMENTED},
                {DTOType.fertilizing, PlantActionType.FERTILIZED},
                {DTOType.watering, PlantActionType.WATERED},
                {DTOType.measurement, PlantActionType.MEASURED},
                {DTOType.photo, PlantActionType.PHOTOGRAPHED},
                {DTOType.blooming, PlantActionType.BLOOMED},
                {DTOType.deceased, PlantActionType.DECEASED},
                {DTOType.harvesting, PlantActionType.HARVESTED},
                {DTOType.misting, PlantActionType.MISTED},
                {DTOType.pollination, PlantActionType.POLLINATED},
                {DTOType.sprouting, PlantActionType.SPROUTED},
                {DTOType.transfer, PlantActionType.TRANSFERRED}
            };
                }
                return _ValidTypes;
            }
        }


        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;

            if (!ValidTypes.ContainsKey(D.EntityType))
                return false;
            if (D.PropertyName == "blobKey")
                return false;

            if (D.PropertyName == "note")
            {
                try
                {
                    string val = (string)D.PropertyValue;
                    if (val != null)
                        this.Note = val;
                }
                catch
                {

                }
            }
            if (D.PropertyName == "value")
            {
                try
                {
                    double? val = (double?)D.PropertyValue;
                    if (val.HasValue)
                        this.Value = val;
                }
                catch
                {

                }
            }

            this.Type = ValidTypes[D.EntityType];


            return base.FromDTO(D);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;

            if (this.Note != null)
            {
                D.PropertyName = "note";
                D.PropertyValue = this.Note ?? "";
            }
            else if (this.Value.HasValue)
            {
                D.PropertyName = "value";
                D.PropertyValue = this.Value.Value;
            }

            D.EntityType = PlantActionPropertySet.ValidTypes
                .Where(x => x.Value == this.Type)
                .Select(x => x.Key)
                .FirstOrDefault();

            base.FillDTO(D);

            D.ParentId = null;

            return true;
        }
    }

    [DTOObject(DTOType.setProperty)]
    public class BlobKeySet : EventBase
    {
        [JsonProperty]
        public string BlobKey { get; private set; }

        [JsonProperty]
        public Photo Pmd { get; private set; }

        protected BlobKeySet() { }

        public BlobKeySet(SetBlobKey cmd)
            : base(cmd)
        {
            this.BlobKey = cmd.BlobKey;
        }

        public override bool FromDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;

            if (D.PropertyName != "blobKey")
                return false;

            this.BlobKey = D.PropertyValue;
            if (D.Pmd != null && D.Pmd.RemoteUri != null)
                this.Pmd = D.Pmd;

            return base.FromDTO(D);
        }

        public override bool FillDTO(IEventDTO Dto)
        {
            var D = Dto as ISetPropertyDTO; if (Dto == null) return false;
            D.PropertyName = "blobKey";
            D.PropertyValue = this.BlobKey;
            D.EntityType = DTOType.photo;

            base.FillDTO(D);

            D.ParentId = null;
            return true;
        }
    }



    #endregion


}

