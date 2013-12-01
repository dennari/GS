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

    public class PlantAction : AggregateBase<PlantActionState, PlantActionCreated>,
        ICommandHandler<CreatePlantAction>,
        ICommandHandler<SetBlobKey>,
        ICommandHandler<SetPlantActionProperty>
    {

        public PlantAction()
        {
            this.UIPersistable = true;
        }

        public void Handle(CreatePlantAction command)
        {
            RaiseEvent(new PlantActionCreated(command));
        }

        public void Handle(SetPlantActionProperty command)
        {

            RaiseEvent(new PlantActionPropertySet(command, this.State.Type));
        }

        public void Handle(SetBlobKey command)
        {

            RaiseEvent(new BlobKeySet(command));
        }

    }


}
