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
            When(new CreatePlant(id));
            Expect(new PlantCreated(id));
        }

        [Test]
        public void given_created_plant()
        {
            Given(new PlantCreated(id));
            When(new CreatePlant(id));
            Expect("rebirth");
        }

    }
}
