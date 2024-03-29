﻿
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using System;



namespace Growthstories.DomainTests
{




    public class add_plant_to_garden : gs_spec
    {

        //[Test]
        //public void test_serialization()
        //{
        //    string json = JsonConvert.SerializeObject(new AddPlant(Guid.Empty, Guid.Empty, "Jore"));
        //    Console.WriteLine(json);
        //    var @event = JsonConvert.DeserializeObject<AddPlant>(json);
        //    json = JsonConvert.SerializeObject(@event);
        //    Console.WriteLine(json);

        //    //Assert.AreEqual(@"{""Id"":""0000-0000-0000-0000""}", json);
        //}

        [Test]
        public void given_created_garden()
        {
            var PlantId = Guid.NewGuid();
            var UserId = Guid.NewGuid();
            Given(new GardenCreated(new CreateGarden(PlantId, UserId)));
            When(new AddPlant(id, PlantId, UserId, "Jore"));
            Expect(new PlantAdded(id, Guid.NewGuid(), PlantId)
            {
                AggregateVersion = 2,
                MessageId = FakeEventFactory.FakeEventId,
                Created = FakeEventFactory.FakeCreated
            });
        }

        [Test]
        public void given_no_prior_history()
        {
            var PlantId = Guid.NewGuid();
            Given();
            When(new AddPlant(id, PlantId, Guid.NewGuid(), "Jore"));
            Expect("premature");
        }

    }
}