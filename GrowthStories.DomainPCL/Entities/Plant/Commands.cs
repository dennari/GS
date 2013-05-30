using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{


    #region Plant
    public class CreatePlant : EntityCommand
    {

        public CreatePlant() { }
        public CreatePlant(Guid id, string name)
            : base(id)
        {
            Name = name;
        }

        public override string ToString()
        {
            return string.Format(@"Create plant {0}.", EntityId);
        }


        public string Name { get; set; }
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


}

