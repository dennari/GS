using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Domain.Entities
{
    public class PlantActionState : AggregateState<PlantActionCreated>
    {

        public PlantActionState() { }
        public PlantActionState(Guid id, int version, bool Public) : base(id, version, Public) { }


    }
}
