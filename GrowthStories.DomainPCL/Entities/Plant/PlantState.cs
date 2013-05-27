using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Domain.Entities
{
    public class PlantState : AggregateState<PlantCreated>
    {

        public PlantState() { }
        public PlantState(Guid id, int version, bool Public) : base(id, version, Public) { }

        public void Apply(MarkedPlantPublic @event)
        {
            Public = true;
        }

        public void Apply(MarkedPlantPrivate @event)
        {
            Public = false;
        }
    }
}
