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




    public class ScheduleState : AggregateState<ScheduleCreated>
    {

        //public Guid UserId { get; private set; }

        public long Interval { get; private set; }

        public ScheduleState()
            : base()
        {
        }

        public ScheduleState(ScheduleCreated e)
            : this()
        {
            this.Apply(e);
        }


        public override void Apply(ScheduleCreated @event)
        {
            base.Apply(@event);
            this.Interval = @event.Interval;
        }



    }
}
