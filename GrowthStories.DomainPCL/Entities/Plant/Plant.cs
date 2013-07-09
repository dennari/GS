
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
        ICommandHandler<MarkPlantPublic>,
        ICommandHandler<MarkPlantPrivate>,
        ICommandHandler<AddComment>,
        ICommandHandler<AddPhoto>,
        ICommandHandler<AddPlant>,
        ICommandHandler<AddWateringAction>,
        ICommandHandler<AddFertilizingAction>,
        ICommandHandler<AddFBComment>
    {

        public new void Create(Guid Id)
        {
            throw new NotSupportedException();
        }

        public void Handle(CreatePlant command)
        {
            if (command.Name == null)
                throw new ArgumentNullException();

            RaiseEvent(new PlantCreated(command.EntityId, command.Name, command.UserId));
        }

        public void Handle(AddComment command)
        {
            RaiseEvent(new CommentAdded()
            {
                EntityId = command.EntityId,
                Note = command.Note
            });
        }

        public void Handle(AddPhoto command)
        {
            RaiseEvent(new PhotoAdded(command.EntityId) { BlobKey = command.BlobKey });
        }

        public void Handle(AddPlant command)
        {
            if (State == null)
            {
                ApplyState(null);
            }
            //RaiseEvent(new PlantCreated(command.PlantId, command.PlantName));
        }

        public void Handle(AddWateringAction command)
        {
            RaiseEvent(new WaterAdded(command.EntityId));

        }

        public void Handle(AddFertilizingAction command)
        {
            RaiseEvent(new FertilizingAdded(command.EntityId));

        }

        public void Handle(AddFBComment command)
        {
            RaiseEvent(new FBCommentAdded(command.EntityId)
            {
                FbId = command.FbId,
                FirstName = command.FirstName,
                LastName = command.LastName,
                Name = command.Name,
                Note = command.Note,
                Uid = command.Uid
            });
        }

        public void Handle(MarkPlantPublic command)
        {
            RaiseEvent(new MarkedPlantPublic(command.EntityId));
        }

        public void Handle(MarkPlantPrivate command)
        {
            RaiseEvent(new MarkedPlantPrivate(command.EntityId));
        }
    }

}
