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
using Ninject;
using Newtonsoft.Json.Schema;
using Growthstories.DomainTests.Sync;
using Newtonsoft.Json.Linq;

namespace Growthstories.DomainTests
{



    public class TranslatorTests
    {

        IKernel kernel;
        [SetUp]
        public void SetUp()
        {
            //if (kernel != null)
            //    kernel.Dispose();
            kernel = new StandardKernel(new TestModule());
            var resolver = new JsonSchemaResolver();
            IEventSchema = JsonSchema.Parse(Schema.ievent, resolver);
            CreateEntitySchema = JsonSchema.Parse(Schema.createevent, resolver);

            User = new AuthUser()
            {
                Id = Guid.NewGuid()
            };


        }

        public string toJSON(object o) { return kernel.Get<IJsonFactory>().Serialize(o); }
        public T fromJSON<T>(string s) { return kernel.Get<IJsonFactory>().Deserialize<T>(s); }

        protected ITranslateEvents Translator
        {
            get
            {
                return kernel.Get<ITranslateEvents>();
            }
        }

        private JsonSchema IEventSchema;
        private JsonSchema CreateEntitySchema;

        protected IAuthUser User { get; set; }


        [Test]
        public void CreateComment()
        {


            var cmd = new CreatePlantAction(
                Guid.NewGuid(),
                User.Id,
                Guid.NewGuid(),
                PlantActionType.COMMENTED,
                "new note")
            {
                AncestorId = User.Id
            };
            var C = new PlantActionCreated(cmd)
            {
                AggregateVersion = 1,
                MessageId = Guid.NewGuid(),
                Created = DateTimeOffset.UtcNow
            };


            PlantActionTests(C, DTOType.addComment);


        }

        [Test]
        public void CreateFbComment()
        {


            var cmd = new CreatePlantAction(
                Guid.NewGuid(),
                User.Id,
                Guid.NewGuid(),
                PlantActionType.COMMENTED,
                "new note")
            {
                AncestorId = User.Id
            };
            var C = new PlantActionCreated(cmd)
            {
                AggregateVersion = 1,
                MessageId = Guid.NewGuid(),
                Created = DateTimeOffset.UtcNow
            };


            PlantActionTests(C, DTOType.addComment);


        }




        [Test]
        public void CreatePhotograph()
        {

            var cmd = new CreatePlantAction(
                Guid.NewGuid(),
                User.Id,
                Guid.NewGuid(),
                PlantActionType.PHOTOGRAPHED,
                "new note")
            {
                Photo = new Photo()
                {
                    BlobKey = "klsdkfsldkf"
                },
                AncestorId = User.Id
            };

            var C = new PlantActionCreated(cmd)
            {
                AggregateVersion = 1,
                MessageId = Guid.NewGuid(),
                Created = DateTimeOffset.UtcNow
            };

            var CC = PlantActionTests(C, DTOType.addPhoto);
            Assert.AreEqual(C.Photo.BlobKey, CC.Photo.BlobKey);

        }

        [Test]
        public void CreateMeasure()
        {

            var cmd = new CreatePlantAction(
                Guid.NewGuid(),
                User.Id,
                Guid.NewGuid(),
                PlantActionType.MEASURED,
                "new note")
            {
                Value = 2342.12,
                MeasurementType = MeasurementType.ILLUMINANCE,
                AncestorId = User.Id
            };

            var C = new PlantActionCreated(cmd)
            {
                AggregateVersion = 1,
                MessageId = Guid.NewGuid(),
                Created = DateTimeOffset.UtcNow
            };

            var CC = PlantActionTests(C, DTOType.addMeasurement);
            Assert.AreEqual(C.Value, CC.Value);
            Assert.AreEqual(C.MeasurementType, CC.MeasurementType);


        }

        [Test]
        public void CreateSchedule()
        {

            var cmd = new CreateSchedule(Guid.NewGuid(), 234234)
            {
                AncestorId = User.Id
            };

            var C = new ScheduleCreated(cmd)
            {
                AggregateVersion = 1,
                MessageId = Guid.NewGuid(),
                Created = DateTimeOffset.UtcNow
            };

            var CC = (ScheduleCreated)Validate(C, DTOType.createIntervalSchedule);
            Assert.AreEqual(C.Interval, CC.Interval);



        }





        protected IEvent Validate(IDomainEvent C, DTOType DTOT)
        {
            var CD = Translator.Out(C);
            var json = toJSON(CD);
            Console.WriteLine(json);
            IList<string> messages;

            JsonSchema schema = IEventSchema;
            //if (C is ICreateEvent && C.HasParent)
            //    schema = CreateEntitySchema;

            Assert.IsTrue(JObject.Parse(json).IsValid(schema, out messages), string.Join("\n\n", messages) + "\n" + json);



            DTOAssertions(C, CD, DTOT, User);



            var CC = Translator.In(fromJSON<EventDTOUnion>(json));
            DTOAssertions(C, CC, DTOT, User);


            return CC;
        }

        protected PlantActionCreated PlantActionTests(PlantActionCreated C, DTOType DTOT)
        {

            var CC = (PlantActionCreated)Validate(C, DTOT);
            Assert.AreEqual(C.Note, CC.Note);
            Assert.AreEqual(C.Type, CC.Type);


            return CC;
        }


        protected void DTOAssertions(IEvent C, object CD, DTOType DT, IAuthUser U)
        {


            var E = CD as IDomainEvent;
            if (E != null)
            {
                Assert.AreEqual(C.AggregateId, E.AggregateId);
                Assert.AreEqual(C.MessageId, E.MessageId);
                Assert.IsTrue(C.GetType() == E.GetType());
                Assert.IsTrue(E.GetDTOType().Contains(DT));
                Assert.AreEqual(C.Created.GetUnixTimestampMillis(), E.Created.GetUnixTimestampMillis());

            }

            var DTO = CD as IEventDTO;
            if (DTO != null)
            {
                Assert.AreEqual(C.AggregateId, DTO.AggregateId);
                Assert.AreEqual(C.MessageId, DTO.MessageId);
                Assert.AreEqual(DT, DTO.EventType);
                Assert.AreEqual(U.Id, DTO.AncestorId);
            }
        }

    }
}
