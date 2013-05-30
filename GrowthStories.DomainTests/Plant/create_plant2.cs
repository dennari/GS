using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Growthstories.Domain.Messaging;

namespace Growthstories.DomainTests
{
    public class create_plant : gs_spec
    {


        [Test]
        public void given_no_prior_history()
        {

            Given();
            When(new CreatePlant(id, "Jore"));
            Expect(new PlantCreated(id, "Jore") { EntityVersion = 1 });
        }

        [Test]
        public void given_created_plant()
        {
            Given(new PlantCreated(id, "Jore"));
            When(new CreatePlant(id, "Jore"));
            Expect("rebirth");
        }

    }
}
