using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;


namespace Growthstories.Domain.Messaging
{


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

