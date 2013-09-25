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

        public Guid UserId { get; private set; }

        public ICollection<Guid> PlantIds { get; private set; }

        public GardenState()
            : base()
        {
            PlantIds = new List<Guid>();
        }

        //public GardenState(Guid id, int version, bool Public, ICollection<Guid> plantIds)
        //    : base(id, version, Public)
        //{
        //    PlantIds = plantIds;
        //}


        public void Apply(PlantAdded @event)
        {
            if (@event.EntityId != Id)
            {
                throw DomainError.Named("ALIEN_ID", "Nonmatching EntityId");
            }
            PlantIds.Add(@event.PlantId);
        }

        public override void Apply(GardenCreated @event)
        {
            base.Apply(@event);
            this.UserId = @event.UserId;
        }

        public void Apply(MarkedGardenPublic @event)
        {
            Public = true;
        }

        public void Apply(MarkedGardenPrivate @event)
        {
            Public = false;
        }


    }
}
