using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{



    #region Garden

    //[DTOObject(DTOType.createGarden)]
    public class GardenCreated : EventBase, ICreateEvent
    {
        [JsonIgnore]
        private Type _AggregateType;
        [JsonIgnore]
        public Type AggregateType
        {
            get { return _AggregateType == null ? _AggregateType = typeof(Garden) : _AggregateType; }
        }

        [JsonProperty]
        public Guid UserId { get; private set; }

        protected GardenCreated() { }
        public GardenCreated(Guid id) : base(id) { }

        public GardenCreated(CreateGarden cmd)
            : this(cmd.EntityId)
        {
            this.UserId = cmd.UserId;
        }

        public override string ToString()
        {
            return string.Format(@"Created garden {0}", EntityId);
        }

    }

    [DTOObject(DTOType.addPlant)]
    public class PlantAdded : EventBase
    {

        [JsonProperty]
        public Guid PlantId { get; private set; }

        [JsonProperty]
        public Guid AncestorId { get; private set; }


        protected PlantAdded() { }
        public PlantAdded(Guid gardenId, Guid plantId)
            : base(gardenId)
        {
            PlantId = plantId;
        }

        public override string ToString()
        {
            return string.Format(@"Added Plant {0} to Garden {1}", PlantId, EntityId);
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IAddPlantDTO)Dto;
            this.PlantId = D.PlantId;
            base.FromDTO(D);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IAddPlantDTO)Dto;
            D.PlantId = this.PlantId;
            base.FromDTO(D);

        }


    }

    public class MarkedGardenPublic : EventBase
    {
        protected MarkedGardenPublic() { }
        public MarkedGardenPublic(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Marked garden {0} public", EntityId);
        }

    }

    public class MarkedGardenPrivate : EventBase
    {
        protected MarkedGardenPrivate() { }
        public MarkedGardenPrivate(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Marked garden {0} private", EntityId);
        }

    }



    #endregion


}

