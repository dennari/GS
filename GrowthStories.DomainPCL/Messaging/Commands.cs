using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{


    public abstract class CommandBase : ICommand
    {

    }

    public abstract class EntityCommand : CommandBase
    {
        public Guid EntityId { get; private set; }
        protected EntityCommand() { }
        public EntityCommand(Guid EntityId)
        {
            this.EntityId = EntityId;
        }
    }



    #region User
    public class CreateUser : EntityCommand
    {


        protected CreateUser() { }
        public CreateUser(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Create user {0}.", EntityId);
        }

    }


    public class DeleteUser : EntityCommand
    {
        protected DeleteUser() { }
        public DeleteUser(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete user {0}.", EntityId);
        }

    }
    #endregion

    #region Plant
    public class CreatePlant : EntityCommand
    {

        public CreatePlant() { }
        public CreatePlant(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Create plant {0}.", EntityId);
        }

    }


    public class MarkPlantPublic : EntityCommand
    {
        protected MarkPlantPublic() { }
        public MarkPlantPublic(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Marked plant {0} public", EntityId);
        }

    }

    public class MarkPlantPrivate : EntityCommand
    {
        protected MarkPlantPrivate() { }
        public MarkPlantPrivate(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Marked plant {0} private", EntityId);
        }

    }

    public class DeletePlant : EntityCommand
    {

        public DeletePlant() { }
        public DeletePlant(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete plant {0}.", EntityId);
        }

    }
    #endregion

    #region Garden
    public class CreateGarden : EntityCommand
    {

        public CreateGarden() { }
        public CreateGarden(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Create garden {0}.", EntityId);
        }

    }

    public class DeleteGarden : EntityCommand
    {

        public DeleteGarden() { }
        public DeleteGarden(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete garden {0}.", EntityId);
        }

    }

    public class AddPlant : EntityCommand
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


    #endregion

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

