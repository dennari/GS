using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{

    public abstract class PlantCommand : EntityCommand<Plant>
    {
        public PlantCommand() { }
        public PlantCommand(Guid EntityId) : base(EntityId) { }
    }

    #region Plant
    public class CreatePlant : PlantCommand, ICreateCommand
    {

        public string Name { get; private set; }

        public string ProfilepicturePath { get; set; }

        public Guid UserId { get; private set; }

        public CreatePlant() { }
        public CreatePlant(Guid id, string name, Guid userId)
            : base(id)
        {
            Name = name;
            this.UserId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Create plant {0}.", EntityId);
        }



    }

    public class ChangeProfilepicturePath : PlantCommand
    {

        public string ProfilepicturePath { get; private set; }

        protected ChangeProfilepicturePath() { }
        public ChangeProfilepicturePath(Guid entityId, string ProfilepicturePath)
            : base(entityId)
        {
            this.ProfilepicturePath = ProfilepicturePath;
        }

        public override string ToString()
        {
            return string.Format(@"Change ProfilepicturePath to {0}", ProfilepicturePath);
        }

    }

    public class MarkPlantPublic : PlantCommand
    {
        protected MarkPlantPublic() { }
        public MarkPlantPublic(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Mark plant {0} public", EntityId);
        }

    }

    public class MarkPlantPrivate : PlantCommand
    {
        protected MarkPlantPrivate() { }
        public MarkPlantPrivate(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Mark plant {0} private", EntityId);
        }

    }

    public class DeletePlant : PlantCommand
    {

        public DeletePlant() { }
        public DeletePlant(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete plant {0}.", EntityId);
        }

    }

    public class AddComment : PlantCommand
    {
        public string Note { get; set; }

        public AddComment() { }
        public AddComment(Guid id, string note)
            : base(id)
        {
            Note = note;
        }

        public override string ToString()
        {
            return string.Format(@"Add comment {0} to plant {1}.", Note, EntityId);
        }

    }

    public class AddPhoto : PlantCommand
    {
        public string BlobKey { get; set; }

        public AddPhoto() { }
        public AddPhoto(Guid id, string BlobKey)
            : base(id)
        {
            this.BlobKey = BlobKey;
        }

        public override string ToString()
        {
            return string.Format(@"Add photo {0} to plant {1}.", BlobKey, EntityId);
        }

    }

    public class AddWateringAction : PlantCommand
    {

        public AddWateringAction() { }
        public AddWateringAction(Guid id)
            : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Add water to plant {1}.", EntityId);
        }

    }

    public class AddFertilizingAction : PlantCommand
    {

        public AddFertilizingAction() { }
        public AddFertilizingAction(Guid id)
            : base(id)
        {
        }

        public override string ToString()
        {
            return string.Format(@"Add fertilizer to plant {1}.", EntityId);
        }

    }

    public class AddFBComment : PlantCommand
    {

        public String FbId { get; set; }
        public long Uid { get; set; }
        public String Name { get; set; }
        public string FirstName { get; set; }
        public String LastName { get; set; }
        public String Note { get; set; }

        public AddFBComment() { }
        public AddFBComment(Guid entityId, string FbId, long Uid, string Name, string FirstName, string LastName, string Note)
            : base(entityId)
        {

            this.FbId = FbId;
            this.Uid = Uid;
            this.Name = Name;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.Note = Note;

        }

        public override string ToString()
        {
            return string.Format(@"Add FBComment {0} to plant {1}", Note, EntityId);
        }


    }


    #endregion


}

