using System;
using Growthstories.Sync;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Growthstories.DomainTests
{
    public class TranslatorTests
    {


        protected ITranslateEvents Translator
        {
            get
            {
                var Translator = new SyncTranslator();
                Translator.Ancestor = User;
                return Translator;
            }
        }

        protected User _User;
        protected User User
        {
            get
            {
                if (_User == null)
                {
                    _User = new User();
                    _User.ApplyState(new UserState(Guid.NewGuid(), 45, true));

                }
                return _User;

            }
        }

        [Test]
        public void Comment()
        {


            var C = new CommentAdded()
            {
                EntityId = Guid.NewGuid(),
                EntityVersion = 235,
                EventId = Guid.NewGuid(),
                Note = "MESSAGE"
            };


            var CD = (IAddCommentDTO)Translator.Out(new List<IDomainEvent>() { C }).ToArray()[0];
            DTOAssertions(C, CD, DTOType.addComment, User);
            Assert.AreEqual(C.Note, CD.Note);

            var CC = (CommentAdded)Translator.In(new List<EventDTOUnion>() { (EventDTOUnion)CD }).ToArray()[0];
            DTOAssertions(C, CC, DTOType.addComment, User);
            Assert.AreEqual(C.Note, CC.Note);



        }

        [Test]
        public void Photo()
        {


            var C = new PhotoAdded()
            {
                EntityId = Guid.NewGuid(),
                EntityVersion = 235,
                EventId = Guid.NewGuid(),
                BlobKey = "MESSAGE"
            };


            var CD = (IAddPhotoDTO)Translator.Out(new List<IDomainEvent>() { C }).ToArray()[0];
            DTOAssertions(C, CD, DTOType.addPhoto, User);
            Assert.AreEqual(C.BlobKey, CD.BlobKey);

            var CC = (PhotoAdded)Translator.In(new List<EventDTOUnion>() { (EventDTOUnion)CD }).ToArray()[0];
            DTOAssertions(C, CC, DTOType.addPhoto, User);
            Assert.AreEqual(C.BlobKey, CC.BlobKey);

        }

        [Test]
        public void MarkedPlantPublic()
        {


            var C = new MarkedPlantPublic()
            {
                EntityId = Guid.NewGuid(),
                EntityVersion = 235,
                EventId = Guid.NewGuid()
            };


            var CD = (ISetPropertyDTO)Translator.Out(new List<IDomainEvent>() { C }).ToArray()[0];
            DTOAssertions(C, CD, DTOType.setProperty, User);
            Assert.AreEqual(Language.SHARED, CD.PropertyName);
            Assert.AreEqual(true, (bool)CD.PropertyValue);
            Assert.AreEqual(DTOType.plant, CD.EntityType);


            var CC = (MarkedPlantPublic)Translator.In(new List<EventDTOUnion>() { (EventDTOUnion)CD }).ToArray()[0];
            DTOAssertions(C, CC, DTOType.setProperty, User);

        }

        [Test]
        public void MarkedPlantPrivate()
        {


            var C = new MarkedPlantPrivate()
            {
                EntityId = Guid.NewGuid(),
                EntityVersion = 235,
                EventId = Guid.NewGuid()
            };


            var CD = (ISetPropertyDTO)Translator.Out(new List<IDomainEvent>() { C }).ToArray()[0];
            Console.WriteLine(JsonConvert.SerializeObject(CD, Formatting.Indented, new StringEnumConverter()));
            DTOAssertions(C, CD, DTOType.setProperty, User);
            Assert.AreEqual(Language.SHARED, CD.PropertyName);
            Assert.AreEqual(false, (bool)CD.PropertyValue);
            Assert.AreEqual(DTOType.plant, CD.EntityType);



            var CC = (MarkedPlantPrivate)Translator.In(new List<EventDTOUnion>() { (EventDTOUnion)CD }).ToArray()[0];
            DTOAssertions(C, CC, 0, User);

        }



        protected void DTOAssertions(IEvent C, IEvent CD, DTOType DT, User U)
        {
            Assert.AreEqual(C.EntityId, CD.EntityId);
            Assert.AreEqual(C.EntityVersion, CD.EntityVersion);
            Assert.AreEqual(C.EventId, CD.EventId);

            var DTO = CD as IEventDTO;
            if (DTO != null)
            {
                Assert.AreEqual(DT, DTO.EventType);
                Assert.AreEqual(U.Id, DTO.AncestorId);
            }

        }
    }
}
