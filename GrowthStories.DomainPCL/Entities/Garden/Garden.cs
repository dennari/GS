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
            RaiseEvent(new PlantAdded()
            {
                EntityId = command.EntityId,
                PlantId = command.PlantId
            });
        }

        public void Handle(CreateGarden command)
        {
            if (State == null)
            {
                ApplyState(null);
            }
            RaiseEvent(new GardenCreated(command.EntityId));
        }

        public void Handle(MarkGardenPublic command)
        {
            RaiseEvent(new MarkedGardenPublic(command.EntityId));
        }
    }


}
