using System;
using NUnit.Framework;
using Growthstories.Domain.Messaging;

namespace Growthstories.DomainTests
{
    public class create_plantaction : gs_spec
    {


        [Test]
        public void given_no_prior_history()
        {
            Given();
            When(new CreatePlantAction(id));
            Expect(new PlantActionCreated(id));
        }

        [Test]
        public void given_created_plantaction()
        {
            Given(new PlantActionCreated(id));
            When(new CreatePlantAction(id));
            Expect("rebirth");
        }

    }
}
