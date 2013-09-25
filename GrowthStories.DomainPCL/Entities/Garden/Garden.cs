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

    public class Garden : AggregateBase<GardenState, GardenCreated>,
        ICommandHandler<AddPlant>,
        ICommandHandler<CreateGarden>,
        ICommandHandler<MarkGardenPublic>
    {


        public void Handle(AddPlant command)
        {
            RaiseEvent(new PlantAdded(command.EntityId, command.PlantId));

        }

        public void Handle(CreateGarden command)
        {
            RaiseEvent(new GardenCreated(command));
        }

        public void Handle(MarkGardenPublic command)
        {
            RaiseEvent(new MarkedGardenPublic(command.EntityId));
        }
    }


}
