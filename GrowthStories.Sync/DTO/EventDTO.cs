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
    public class EventDTOUnion : IEventDTO, IAddEntityDTO, IAddCommentDTO, IAddFertilizingDTO, IAddIntervalScheduleDTO,
        IAddMeasurementDTO, IAddPhotoDTO, ICreatePlantDTO, IAddPlantDTO, ICreateGardenDTO, IAddRelationshipDTO,
        ICreateUserDTO, IDelEntityDTO, IDelPropertyDTO, ISetPropertyDTO
    {
        #region IEvent
        [JsonProperty(PropertyName = Language.EVENT_ID, Required = Required.Always)]
        public Guid EventId { get; set; }

        [JsonProperty(PropertyName = Language.ENTITY_ID, Required = Required.Always)]
        public Guid EntityId { get; set; }

        [JsonProperty(PropertyName = Language.ENTITY_VERSION, Required = Required.Always)]
        public int EntityVersion { get; set; }
        #endregion

        #region IEventDTO
        [JsonProperty(PropertyName = Language.ANCESTOR_ID, Required = Required.Always)]
        public Guid AncestorId { get; set; }

        [JsonProperty(PropertyName = Language.EVENT_TYPE, Required = Required.Always)]
        public DTOType EventType { get; set; }

        [JsonProperty(PropertyName = Language.CREATED, Required = Required.Always)]
        public DateTimeOffset Created { get; set; }

        [JsonProperty(PropertyName = Language.STREAM_ENTITY, Required = Required.Always)]
        public Guid StreamEntity { get; set; }
        #endregion

        #region IAddEntityDTO
        [JsonProperty(PropertyName = Language.PARENT_ID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid ParentId { get; set; }

        [JsonProperty(PropertyName = Language.PARENT_ANCESTOR_ID, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid ParentAncestorId { get; set; }
        #endregion

        #region ISetPropertyDTO
        [JsonProperty(PropertyName = Language.ENTITY_TYPE, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DTOType EntityType { get; set; }

        [JsonProperty(PropertyName = Language.PROPERTY_NAME, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PropertyName { get; set; }

        [JsonProperty(PropertyName = Language.PROPERTY_VALUE, Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public dynamic PropertyValue { get; set; }
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


    }






}