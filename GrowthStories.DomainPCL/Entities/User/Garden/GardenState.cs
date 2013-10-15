using CommonDomain;
using EventStore;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Entities
{




    public class GardenState : EntityState<GardenCreated>
    {



        public IList<Guid> PlantIds { get; private set; }

        public GardenState()
            : base()
        {
            PlantIds = new List<Guid>();
        }

        public GardenState(GardenCreated @event)
            : this()
        {
            // TODO: Complete member initialization
            Apply(@event);
        }


        public void Apply(PlantAdded @event)
        {

            PlantIds.Add(@event.PlantId);
        }




    }
}
