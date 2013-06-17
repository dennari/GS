using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{



    #region Plant

    public class PlantCreated : EventBase
    {

        public string Name { get; set; }
        protected PlantCreated() { }
        public PlantCreated(Guid entityId, string name)
            : base(entityId)
        {
            if (name == null)
            {
                throw new ArgumentNullException("a name has to be provided");
            }
            Name = name;
        }

        public override string ToString()
        {
            return string.Format(@"Created plant {0}, {1}", Name, EntityId);
        }



    }

    [DTOObject(DTOType.setProperty)]
    public class MarkedPlantPublic : EventBase
    {
        public MarkedPlantPublic() { }
        public MarkedPlantPublic(Guid entityId) : base(entityId) { }


        public override string ToString()
        {
            return string.Format(@"Mark plant {0} public", EntityId);
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
        public CommentAdded(Guid entityId, string note) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Marked plant {0} private", EntityId);
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
            return string.Format(@"Marked plant {0} private", EntityId);
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

    #endregion


}

