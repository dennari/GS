using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{

    public abstract class GardenCommand : EntityCommand<Garden>
    {
        protected GardenCommand() { }
        public GardenCommand(Guid EntityId) : base(EntityId) { }
    }

    #region Garden
    public class CreateGarden : GardenCommand
    {

        protected CreateGarden() { }
        public CreateGarden(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Create garden {0}.", EntityId);
        }

    }

    public class DeleteGarden : GardenCommand
    {

        protected DeleteGarden() { }
        public DeleteGarden(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete garden {0}.", EntityId);
        }

    }

    public class AddPlant : GardenCommand
    {

        public Guid PlantId { get; private set; }
        public string PlantName { get; private set; }

        protected AddPlant() { }
        public AddPlant(Guid id, Guid PlantId, string PlantName)
            : base(id)
        {
            this.PlantId = PlantId;
            this.PlantName = PlantName;
        }

        public override string ToString()
        {
            return string.Format(@"Add Plant {0} to Garden {1}", PlantId, EntityId);
        }



    }
    public class MarkGardenPublic : GardenCommand
    {
        protected MarkGardenPublic() { }
        public MarkGardenPublic(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Marked garden {0} public", EntityId);
        }

    }




    #endregion


}

