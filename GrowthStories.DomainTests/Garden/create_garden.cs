
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using NUnit.Framework;
using System;


namespace Growthstories.DomainTests
{




    public class create_garden : gs_spec
    {


        [Test]
        public void given_no_prior_history()
        {
            var cmd = new CreateGarden(id, Guid.NewGuid());
            Given();
            When(cmd);
            Expect(new GardenCreated(cmd)
            {
                AggregateVersion = 1,
                MessageId = FakeEventFactory.FakeEventId,
                Created = FakeEventFactory.FakeCreated
            });
        }

        [Test]
        public void given_created_garden()
        {
            Given(new GardenCreated(new CreateGarden(id, Guid.NewGuid())) { AggregateVersion = 1 });
            When(new CreateGarden(id, Guid.NewGuid()));
            Expect("rebirth");
        }

    }
}