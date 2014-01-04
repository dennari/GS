using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Growthstories.Sync
{

    //[JsonObject()]
    public class EventDTOUnion : IEventDTO, IAddEntityDTO, ICreatePlantActionDTO, IAddIntervalScheduleDTO,
        ICreatePlantDTO, IAddPlantDTO, ICreateGardenDTO, IAddRelationshipDTO,
        ICreateUserDTO, IDelEntityDTO, IDelPropertyDTO, ISetPropertyDTO, IRegisterDTO
    {
        #region IEvent
        [JsonProperty(PropertyName = Language.EVENT_ID, Required = Required.Always)]
        public Guid MessageId { get; set; }


        [JsonProperty(PropertyName = Language.STREAM_ENTITY, Required = Required.Always)]
        public Guid AggregateId { get; set; }

        [JsonProperty(PropertyName = Language.ENTITY_VERSION, Required = Required.Always)]
        public int AggregateVersion { get; set; }

        [JsonProperty(PropertyName = Language.ENTITY_ID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? EntityId { get; set; }


        #endregion


        #region IEventDTO
        [JsonProperty(PropertyName = Language.ANCESTOR_ID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? AncestorId { get; set; }

        [JsonProperty(PropertyName = Language.EVENT_TYPE, Required = Required.Always)]
        public DTOType EventType { get; set; }

        [JsonProperty(PropertyName = Language.CREATED, Required = Required.Always)]
        public long Created { get; set; }


        [JsonProperty(PropertyName = Language.STREAM_ANCESTOR, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? StreamAncestor { get; set; }

        [JsonProperty(PropertyName = Language.PARENT_ID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? ParentId { get; set; }

        [JsonProperty(PropertyName = Language.PARENT_ANCESTOR_ID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid? ParentAncestorId { get; set; }
        #endregion

        #region IAddEntityDTO
        #endregion

        [JsonProperty(PropertyName = Language.ENTITY_TYPE, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DTOType EntityType { get; set; }

        #region ISetPropertyDTO
        [JsonProperty(PropertyName = Language.PROPERTY_ENTITY_ID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid PropertyEntityId { get; set; }

        [JsonProperty(PropertyName = Language.PROPERTY_NAME, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PropertyName { get; set; }

        [JsonProperty(PropertyName = Language.PROPERTY_VALUE, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public dynamic PropertyValue { get; set; }


        [JsonProperty(PropertyName = "pmd", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Photo Pmd { get; set; }

        #endregion

        [JsonProperty(PropertyName = Language.NOTE, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Note { get; set; }

        [JsonProperty(PropertyName = Language.PLANT_NAME, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }


        [JsonProperty(PropertyName = Language.INTERVAL, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Interval { get; set; }

        [JsonProperty(PropertyName = Language.BLOB_KEY, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BlobKey { get; set; }

        [JsonProperty(PropertyName = Language.PLANT_ID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid PlantId { get; set; }

        [JsonProperty(PropertyName = Language.PLANT_ANCESTOR_ID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid PlantAncestorId { get; set; }

        [JsonProperty(PropertyName = Language.TO, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid To { get; set; }

        [JsonProperty(PropertyName = Language.USERNAME, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty(PropertyName = Language.PASSWORD, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Password { get; set; }

        [JsonProperty(PropertyName = Language.EMAIL, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(PropertyName = Language.MEASUREMENT_TYPE, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MeasurementType MeasurementType { get; set; }

        [JsonProperty(PropertyName = Language.VALUE, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double Value { get; set; }


        [JsonProperty(PropertyName = Language.FBUID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]

        public long FBUid { get; set; }

        [JsonProperty(PropertyName = Language.FBNAME, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FBName { get; set; }

    }






}