using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{

    public abstract class GardenCommand : AggregateCommand<User>
    {
        protected GardenCommand() { }
        public GardenCommand(Guid UserId, Guid GardenId) : base(UserId, GardenId) { }
    }

    #region Garden
    public class CreateGarden : GardenCommand
    {


        protected CreateGarden() { }
        public CreateGarden(Guid id, Guid userId)
            : base(userId, id)
        {
        }

        public override string ToString()
        {
            return string.Format(@"Create garden {0}.", EntityId);
        }

    }

    public class DeleteGarden : GardenCommand
    {


        protected DeleteGarden() { }
        public DeleteGarden(Guid id, Guid userId)
            : base(userId, id)
        {
        }

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
        public AddPlant(Guid gardenId, Guid PlantId, Guid userId, string PlantName)
            : base(userId, gardenId)
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
        public MarkGardenPublic(Guid id, Guid userId)
            : base(userId, id)
        {
        }

        public override string ToString()
        {
            return string.Format(@"Marked garden {0} public", AggregateId);
        }

    }

    public class MarkGardenPrivate : GardenCommand
    {

        protected MarkGardenPrivate() { }
        public MarkGardenPrivate(Guid id, Guid userId)
            : base(userId, id)
        {
        }

        public override string ToString()
        {
            return string.Format(@"Marked garden {0} private", AggregateId);
        }

    }



    #endregion


}

