using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{



    #region Plant

    [DTOObject(DTOType.createPlant)]
    public class PlantCreated : EventBase
    {

        public string Name { get; set; }
        public Guid UserId { get; set; }

        protected PlantCreated() { }
        public PlantCreated(Guid entityId, string name, Guid userId)
            : base(entityId)
        {
            if (name == null)
            {
                throw new ArgumentNullException("a name has to be provided");
            }
            if (userId == default(Guid))
            {
                throw new ArgumentNullException("userId has to be provided");
            }
            Name = name;
            UserId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Created plant {0}, {1}", Name, EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ICreatePlantDTO)Dto;
            base.FillDTO(D);
            D.Name = this.Name;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ICreatePlantDTO)Dto;
            base.FromDTO(D);
            this.Name = D.Name;
        }

    }

    [DTOObject(DTOType.setProperty)]
    public class MarkedPlantPublic : EventBase
    {
        public MarkedPlantPublic() { }
        public MarkedPlantPublic(Guid entityId) : base(entityId) { }


        public override string ToString()
        {
            return string.Format(@"Marked plant {0} public", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            base.FillDTO(D);
            D.PropertyName = Language.SHARED;
            D.PropertyValue = true;
            D.EntityType = DTOType.plant;

        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            if (D.PropertyName != Language.SHARED)
                throw new ArgumentException();
            if ((bool)D.PropertyValue != true)
                throw new ArgumentException();
            if (D.EntityType != DTOType.plant)
                throw new ArgumentException();
            base.FromDTO(D);

        }



    }

    [DTOObject(DTOType.setProperty)]
    public class MarkedPlantPrivate : EventBase
    {
        public MarkedPlantPrivate() { }
        public MarkedPlantPrivate(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Marked plant {0} private", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            base.FillDTO(D);
            D.PropertyName = Language.SHARED;
            D.PropertyValue = false;
            D.EntityType = DTOType.plant;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (ISetPropertyDTO)Dto;
            if (D.PropertyName != Language.SHARED)
                throw new ArgumentException();
            if ((bool)D.PropertyValue != false)
                throw new ArgumentException();
            if (D.EntityType != DTOType.plant)
                throw new ArgumentException();
            base.FromDTO(D);

        }

    }


    [DTOObject(DTOType.addComment)]
    public class CommentAdded : EventBase
    {

        public string Note { get; set; }

        public CommentAdded() { }

        public override string ToString()
        {
            return string.Format(@"Added comment {0} to plant {1}", Note, EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IAddCommentDTO)Dto;
            base.FillDTO(D);
            D.Note = Note;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IAddCommentDTO)Dto;
            this.Note = D.Note;
            base.FromDTO(D);

        }
    }

    [DTOObject(DTOType.addPhoto)]
    public class PhotoAdded : EventBase
    {

        public string BlobKey { get; set; }

        public PhotoAdded() { }
        public PhotoAdded(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Added photo with BlobKey {0} to plant {0} private", BlobKey, EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IAddPhotoDTO)Dto;
            base.FillDTO(D);
            D.BlobKey = this.BlobKey;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IAddPhotoDTO)Dto;
            this.BlobKey = D.BlobKey;
            base.FromDTO(D);

        }


    }

    [DTOObject(DTOType.addFertilizing)]
    public class FertilizingAdded : EventBase
    {


        public FertilizingAdded() { }
        public FertilizingAdded(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Added fertilizing to plant {0}", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IAddFertilizingDTO)Dto;
            base.FillDTO(D);
            //D.
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IAddFertilizingDTO)Dto;
            base.FromDTO(D);

        }


    }

    [DTOObject(DTOType.addWatering)]
    public class WaterAdded : EventBase
    {


        public WaterAdded() { }
        public WaterAdded(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Added water to plant {0}", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IAddWateringDTO)Dto;
            base.FillDTO(D);
            //D.
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IAddWateringDTO)Dto;
            base.FromDTO(D);

        }
    }

    [DTOObject(DTOType.addFBComment)]
    public class FBCommentAdded : EventBase
    {

        public String FbId { get; set; }
        public long Uid { get; set; }
        public String Name { get; set; }
        public string FirstName { get; set; }
        public String LastName { get; set; }
        public String Note { get; set; }

        public FBCommentAdded() { }
        public FBCommentAdded(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Added water to plant {0}", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IAddFBCommentDTO)Dto;
            D.FbId = this.FbId;
            D.Uid = this.Uid;
            D.Name = this.Name;
            D.FirstName = this.FirstName;
            D.LastName = this.LastName;
            D.Note = this.Note;
            base.FillDTO(D);
            //D.
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IAddFBCommentDTO)Dto;
            this.FbId = D.FbId;
            this.Uid = D.Uid;
            this.Name = D.Name;
            this.FirstName = D.FirstName;
            this.LastName = D.LastName;
            this.Note = D.Note;
            base.FromDTO(D);

        }
    }



    #endregion


}

