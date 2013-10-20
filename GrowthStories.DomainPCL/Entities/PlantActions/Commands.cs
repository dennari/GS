using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{

    public abstract class PlantActionCommand : AggregateCommand<PlantAction>
    {

        public PlantActionCommand(Guid AggregateId) : base(AggregateId) { }
    }

    #region PlantAction
    public class CreatePlantAction : PlantActionCommand, ICreateMessage
    {

        public PlantActionType Type { get; private set; }

        public Guid UserId { get; private set; }

        public Guid PlantId { get; private set; }

        public string Note { get; private set; }

        public MeasurementType MeasurementType { get; set; }

        public double? Value { get; set; }

        public Photo? Photo { get; set; }

        //protected CreatePlantAction() { }
        public CreatePlantAction(Guid id, Guid userId, Guid plantId, PlantActionType type, string note)
            : base(id)
        {
            this.UserId = userId;
            this.PlantId = plantId;
            this.Type = type;
            this.Note = note;

            this.AncestorId = userId;
            this.ParentId = plantId;
            this.ParentAncestorId = userId;
            this.StreamAncestorId = userId;
            this.StreamEntityId = plantId;

        }

        public override string ToString()
        {
            return string.Format(@"Create PlantAction {0}.", AggregateId);
        }

    }

    public class DeletePlantAction : PlantActionCommand
    {

        //protected DeletePlantAction() { }
        public DeletePlantAction(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete PlantAction {0}.", AggregateId);
        }

    }

    public class SetPlantActionProperty : PlantActionCommand
    {
        //public PlantActionType Type { get; private set; }

        public string Note { get; set; }

        public MeasurementType MeasurementType { get; set; }

        public double? Value { get; set; }

        public Photo? Photo { get; set; }


        //protected SetPlantActionProperty() { }
        public SetPlantActionProperty(Guid id)
            : base(id)
        {

        }
    }

    #endregion


}

