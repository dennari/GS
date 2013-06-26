
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using NUnit.Framework;


namespace Growthstories.DomainTests
{




    public class create_garden : gs_spec
    {


        [Test]
        public void given_no_prior_history()
        {
            Given();
            When(new CreateGarden(id));
            Expect(new GardenCreated(id)
            {
                EntityVersion = 1,
                EventId = FakeEventFactory.FakeEventId,
                Created = FakeEventFactory.FakeCreated
            });
        }

        [Test]
        public void given_created_garden()
        {
            Given(new GardenCreated(id) { EntityVersion = 1 });
            When(new CreateGarden(id));
            Expect("rebirth");
        }

    }
}