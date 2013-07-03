using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;


namespace Growthstories.Domain.Entities
{

    public class SynchronizerState : AggregateState<SynchronizerCreated>
    {

        public SynchronizerState() { }

        public void Apply(Synchronized @event)
        {
        }

        public void Apply(UserSynchronized @event)
        {
        }


    }
}
