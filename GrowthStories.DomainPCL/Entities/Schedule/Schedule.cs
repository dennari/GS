using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using Growthstories.Domain.Messaging;
using Growthstories.Core;
using EventStore;
using CommonDomain;

namespace Growthstories.Domain.Entities
{

    public class Schedule : AggregateBase<ScheduleState, ScheduleCreated>,
        ICommandHandler<CreateSchedule>
    {


        public void Handle(CreateSchedule command)
        {
            RaiseEvent(new ScheduleCreated(command));
        }

    }


}
