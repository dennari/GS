
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
        ICommandHandler<SetWateringSchedule>,
        ICommandHandler<SetFertilizingSchedule>,
        ICommandHandler<SetTags>,
        ICommandHandler<SetName>,
        ICommandHandler<SetSpecies>,
        ICommandHandler<SetProfilepicture>,
        ICommandHandler<MarkPlantPublic>,
        ICommandHandler<MarkPlantPrivate>
    {



        public void Handle(CreatePlant command)
        {
            if (command.Name == null)
                throw new ArgumentNullException();

            RaiseEvent(new PlantCreated(command));
        }


        public void Handle(SetWateringSchedule command)
        {

            RaiseEvent(new WateringScheduleSet(command));
        }

        public void Handle(SetFertilizingSchedule command)
        {

            RaiseEvent(new FertilizingScheduleSet(command));
        }

        public void Handle(SetTags command)
        {

            RaiseEvent(new TagsSet(command));
        }

        public void Handle(SetName command)
        {

            RaiseEvent(new NameSet(command));
        }

        public void Handle(SetSpecies command)
        {

            RaiseEvent(new SpeciesSet(command));
        }

        public void Handle(MarkPlantPublic command)
        {
            RaiseEvent(new MarkedPlantPublic(command.EntityId));
        }

        public void Handle(MarkPlantPrivate command)
        {
            RaiseEvent(new MarkedPlantPrivate(command.EntityId));
        }

        public void Handle(SetProfilepicture command)
        {
            RaiseEvent(new ProfilepictureSet(command));
        }
    }

}
