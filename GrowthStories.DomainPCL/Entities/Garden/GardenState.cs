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




    public class GardenState : AggregateState<GardenCreated>
    {


        public ICollection<Guid> PlantIds { get; private set; }

        public GardenState()
            : base()
        {
            PlantIds = new List<Guid>();
        }

        public GardenState(Guid id, int version, bool Public, ICollection<Guid> plantIds)
            : base(id, version, Public)
        {
            PlantIds = plantIds;
        }


        public void Apply(PlantAdded @event)
        {
            if (@event.EntityId != Id)
            {
                throw DomainError.Named("ALIEN_ID", "Nonmatching EntityId");
            }
            PlantIds.Add(@event.PlantId);
        }


    }
}
