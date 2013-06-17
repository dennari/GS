using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;


namespace Growthstories.Domain.Messaging
{



    #region Garden

    public class GardenCreated : EventBase
    {

        public GardenCreated() { }
        public GardenCreated(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Created garden {0}", EntityId);
        }

    }

    public class PlantAdded : EventBase
    {

        public Guid PlantId { get; set; }


        public Guid AncestorId { get; set; }


        public string PlantName { get; set; }

        public PlantAdded() { }
        public PlantAdded(Guid gardenId, Guid plantId, string plantName)
            : base(gardenId)
        {
            PlantId = plantId;
            PlantName = plantName;
        }

        public override string ToString()
        {
            return string.Format(@"Added Plant {0} to Garden {1}", PlantId, EntityId);
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

