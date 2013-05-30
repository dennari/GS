using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;


namespace Growthstories.Domain.Messaging
{



    #region Plant

    public class PlantCreated : EventBase
    {

        protected PlantCreated() { }
        public PlantCreated(Guid entityId, string name)
            : base(entityId)
        {
            if (name == null)
            {
                throw new ArgumentNullException("a name has to be provided");
            }
            Name = name;
        }

        public override string ToString()
        {
            return string.Format(@"Created plant {0}, {1}", Name, EntityId);
        }


        public string Name { get; set; }
    }

    public class MarkedPlantPublic : EventBase
    {
        protected MarkedPlantPublic() { }
        public MarkedPlantPublic(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Mark plant {0} public", EntityId);
        }

    }


    public class MarkedPlantPrivate : EventBase
    {
        protected MarkedPlantPrivate() { }
        public MarkedPlantPrivate(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Marked plant {0} private", EntityId);
        }

    }

    #endregion


}

