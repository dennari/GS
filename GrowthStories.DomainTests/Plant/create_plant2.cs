using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Growthstories.Domain.Messaging;
using Growthstories.Core;

namespace Growthstories.DomainTests
{
    public class create_plant : gs_spec
    {


        [Test]
        public void given_no_prior_history()
        {
            var UserId = Guid.NewGuid();

            Given();
            When(new CreatePlant(id, "Jore", UserId));
            Expect(new PlantCreated(id, "Jore", UserId)
            {
                AggregateVersion = 1,
                MessageId = FakeEventFactory.FakeEventId,
                Created = FakeEventFactory.FakeCreated
            });
        }

        [Test]
        public void given_created_plant()
        {
            var UserId = Guid.NewGuid();
            Given(new PlantCreated(id, "Jore", UserId));
            When(new CreatePlant(id, "Jore", UserId));
            Expect("rebirth");
        }

    }
}
