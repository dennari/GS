using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{

    public abstract class GardenCommand : EntityCommand<User>
    {
        protected GardenCommand() { }
        public GardenCommand(Guid EntityId) : base(EntityId) { }
    }

    #region Garden
    public class CreateGarden : GardenCommand
    {

        public Guid UserId { get; set; }

        protected CreateGarden() { }
        public CreateGarden(Guid id, Guid userId)
            : base(id)
        {
            UserId = userId;
            ParentId = userId;
            StreamEntityId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Create garden {0}.", EntityId);
        }

    }

    public class DeleteGarden : GardenCommand
    {

        public Guid UserId { get; set; }

        protected DeleteGarden() { }
        public DeleteGarden(Guid id, Guid userId)
            : base(id)
        {
            UserId = userId;
            ParentId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Delete garden {0}.", EntityId);
        }

    }

    public class AddPlant : GardenCommand
    {

        public Guid PlantId { get; private set; }
        public Guid UserId { get; set; }
        public string PlantName { get; private set; }

        protected AddPlant() { }
        public AddPlant(Guid gardenId, Guid PlantId, Guid userId, string PlantName)
            : base(gardenId)
        {
            this.PlantId = PlantId;
            this.PlantName = PlantName;
            UserId = userId;
            ParentId = userId;
            this.StreamEntityId = userId;
            this.AncestorId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Add Plant {0} to Garden {1}", PlantId, EntityId);
        }



    }
    public class MarkGardenPublic : GardenCommand
    {
        public Guid UserId { get; set; }

        protected MarkGardenPublic() { }
        public MarkGardenPublic(Guid id, Guid userId)
            : base(id)
        {
            UserId = userId;
            ParentId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Marked garden {0} public", EntityId);
        }

    }

    public class MarkGardenPrivate : GardenCommand
    {
        public Guid UserId { get; set; }

        protected MarkGardenPrivate() { }
        public MarkGardenPrivate(Guid id, Guid userId)
            : base(id)
        {
            UserId = userId;
            ParentId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Marked garden {0} private", EntityId);
        }

    }



    #endregion


}

