using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Growthstories.Domain.Messaging;
using Growthstories.Core;

namespace Growthstories.DomainTests
{
    public class mark_plant_public_or_private : gs_spec
    {


        [Test]
        public void given_created_plant_mark_public()
        {
            var UserId = Guid.NewGuid();
            Given(new PlantCreated(id, "Jore", UserId));
            When(new MarkPlantPublic(id));
            Expect(new MarkedPlantPublic(id)
            {
                AggregateVersion = 2,
                MessageId = FakeEventFactory.FakeEventId,
                Created = FakeEventFactory.FakeCreated
            });
        }

        [Test]
        public void given_created_plant_mark_private()
        {
            var UserId = Guid.NewGuid();
            Given(new PlantCreated(id, "Jore", UserId));
            When(new MarkPlantPrivate(id));
            Expect(new MarkedPlantPrivate(id)
            {
                AggregateVersion = 2,
                MessageId = FakeEventFactory.FakeEventId,
                Created = FakeEventFactory.FakeCreated
            });
        }

    }
}
