using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{

    #region PlantAction
    public class CreatePlantAction : EntityCommand
    {

        public CreatePlantAction() { }
        public CreatePlantAction(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Create plant action {0}.", EntityId);
        }

    }


    public class DeletePlantAction : EntityCommand
    {

        public DeletePlantAction() { }
        public DeletePlantAction(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete plant action {0}.", EntityId);
        }

    }
    #endregion

}

