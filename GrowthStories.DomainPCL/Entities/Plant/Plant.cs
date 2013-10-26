
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
        public Plant()
        {
            this.SyncStreamType = Core.PullStreamType.PLANT;
            this.UIPersistable = true;
        }


        public void Handle(CreatePlant command)
        {
            if (command.Name == null)
                throw new ArgumentNullException();

            RaiseEvent(new PlantCreated(command));
        }


        public void Handle(SetWateringSchedule command)
        {
            if (command.AggregateId != this.State.Id)
                throw new InvalidOperationException("command was misrouted.");
            RaiseEvent(new ScheduleSet(command));
        }

        public void Handle(SetFertilizingSchedule command)
        {

            if (command.AggregateId != this.State.Id)
                throw new InvalidOperationException("command was misrouted.");
            RaiseEvent(new ScheduleSet(command));
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
            RaiseEvent(new MarkedPlantPublic(command.AggregateId));
        }

        public void Handle(MarkPlantPrivate command)
        {
            RaiseEvent(new MarkedPlantPrivate(command.AggregateId));
        }

        public void Handle(SetProfilepicture command)
        {
            RaiseEvent(new ProfilepictureSet(command));
        }
    }

}
