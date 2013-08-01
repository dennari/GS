﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using Growthstories.Domain.Messaging;
using CommonDomain;
using CommonDomain.Core;
using Growthstories.Core;

namespace Growthstories.Domain.Entities
{

    public class Plant : AggregateBase<PlantState, PlantCreated>,
        ICommandHandler<CreatePlant>,
        ICommandHandler<ChangeProfilepicturePath>,
        ICommandHandler<MarkPlantPublic>,
        ICommandHandler<MarkPlantPrivate>
    {



        public void Handle(CreatePlant command)
        {
            if (command.Name == null)
                throw new ArgumentNullException();

            RaiseEvent(new PlantCreated(command));
        }



        public void Handle(AddPlant command)
        {
            if (State == null)
            {
                ApplyState(null);
            }
            //RaiseEvent(new PlantCreated(command.PlantId, command.PlantName));
        }


        public void Handle(MarkPlantPublic command)
        {
            RaiseEvent(new MarkedPlantPublic(command.EntityId));
        }

        public void Handle(MarkPlantPrivate command)
        {
            RaiseEvent(new MarkedPlantPrivate(command.EntityId));
        }

        public void Handle(ChangeProfilepicturePath command)
        {
            RaiseEvent(new ProfilepicturePathChanged(command));
        }
    }

}
