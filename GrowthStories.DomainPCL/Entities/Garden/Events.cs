using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Growthstories.Sync;
using Newtonsoft.Json.Linq;


namespace Growthstories.Domain.Messaging
{



    #region Garden

    [DTOObject(DTOType.createGarden)]
    public class GardenCreated : EventBase
    {

        [JsonProperty]
        public Guid UserId { get; private set; }

        protected GardenCreated() { }
        public GardenCreated(Guid id) : base(id) { }

        public GardenCreated(CreateGarden cmd)
            : base(cmd)
        {
            this.UserId = cmd.UserId;

        }

        public override string ToString()
        {
            return string.Format(@"Created garden {0}", EntityId);
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ICreateGardenDTO)Dto;

            base.FromDTO(D);

            this.UserId = this.StreamEntityId ?? default(Guid);

        }



    }

    [DTOObject(DTOType.setProperty)]
    public class PlantAdded : EventBase
    {

        [JsonProperty]
        public Guid PlantId { get; private set; }


        [JsonProperty]
        public Guid UserId { get; private set; }


        protected PlantAdded() { }
        public PlantAdded(Guid gardenId, Guid plantId)
            : base(gardenId)
        {
            PlantId = plantId;

        }

        public PlantAdded(AddPlant cmd)
            : base(cmd)
        {
            PlantId = cmd.PlantId;
            UserId = cmd.UserId;
        }

        public override string ToString()
        {
            return string.Format(@"Added Plant {0} to Garden {1}", PlantId, EntityId);
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            if (D.EntityType != DTOType.garden || D.PropertyName != "plants")
                throw new ArgumentException();


            base.FromDTO(D);

            try
            {
                var val = (JObject)D.PropertyValue;
                this.PlantId = Guid.Parse(val[Language.PROPERTY_ENTITY_ID].ToString());
                this.UserId = Guid.Parse(val[Language.PROPERTY_ANCESTOR_ID].ToString());
            }
            catch
            {

            }

        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            D.EntityType = DTOType.garden;
            D.PropertyName = "plants";
            D.PropertyValue = new JObject();
            D.PropertyValue[Language.PROPERTY_ANCESTOR_ID] = this.UserId.ToString();
            D.PropertyValue[Language.PROPERTY_ENTITY_ID] = this.PlantId.ToString();

            base.FillDTO(D);

            D.ParentId = null;
        }


    }

    public class MarkedGardenPublic : EventBase
    {
        [JsonProperty]
        public Guid UserId { get; private set; }

        protected MarkedGardenPublic() { }
        public MarkedGardenPublic(Guid entityId) : base(entityId) { }
        public MarkedGardenPublic(MarkGardenPublic cmd)
            : base(cmd)
        {
            UserId = cmd.UserId;
        }


        public override string ToString()
        {
            return string.Format(@"Marked garden {0} public", EntityId);
        }

    }

    public class MarkedGardenPrivate : EventBase
    {
        [JsonProperty]
        public Guid UserId { get; private set; }

        protected MarkedGardenPrivate() { }
        public MarkedGardenPrivate(Guid entityId) : base(entityId) { }
        public MarkedGardenPrivate(MarkGardenPrivate cmd)
            : base(cmd)
        {
            UserId = cmd.UserId;
        }

        public override string ToString()
        {
            return string.Format(@"Marked garden {0} private", EntityId);
        }

    }



    #endregion


}

