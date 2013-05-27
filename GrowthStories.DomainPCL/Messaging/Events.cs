using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;


namespace Growthstories.Domain.Messaging
{


    public interface IDomainEvent : IEvent
    {
        //public long EntityVersion { get; }
    }


    public abstract class EventBase : IDomainEvent
    {
        public Guid EntityId { get; set; }
        //public long EntityVersion { get; set; }
        //public Guid EventId { get; set; }

        protected EventBase() { }

        public EventBase(Guid entityId)
        {
            EntityId = entityId;
        }

        //public EventBase(Guid eventId, Guid entityId, long entityVersion)
        //{
        //    EntityId = entityId;
        //    EventId = eventId;
        //    EntityVersion = entityVersion;
        //}

        //public EventBase(Guid eventId, Guid entityId)
        //    : this(eventId, entityId, 0)
        //{

        //}

    }


    #region User


    public class UserCreated : EventBase
    {

        protected UserCreated() { }
        public UserCreated(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Created user {0}", EntityId);
        }

    }

    #endregion

    #region Plant

    public class PlantCreated : EventBase
    {
        protected PlantCreated() { }
        public PlantCreated(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Created plant {0}", EntityId);
        }

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

    #region Garden

    public class GardenCreated : EventBase
    {

        protected GardenCreated() { }
        public GardenCreated(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Created garden {0}", EntityId);
        }

    }

    public class PlantAdded : EventBase
    {

        public Guid PlantId { get; private set; }
        public string PlantName { get; private set; }

        //protected PlantAdded() { }
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



    #endregion

    #region PlantAction

    public class PlantActionCreated : EventBase
    {

        public PlantActionCreated() { }
        public PlantActionCreated(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Created plant action {0}", EntityId);
        }

    }


    #endregion

}

