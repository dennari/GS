using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Growthstories.Domain.Messaging;

namespace Growthstories.DomainTests
{
    public class mark_plant_public_or_private : gs_spec
    {


        [Test]
        public void given_created_plant_mark_public()
        {
            Given(new PlantCreated(id));
            When(new MarkPlantPublic(id));
            Expect(new MarkedPlantPublic(id));
        }

        [Test]
        public void given_created_plant_mark_private()
        {
            Given(new PlantCreated(id));
            When(new MarkPlantPrivate(id));
            Expect(new MarkedPlantPrivate(id));
        }

    }
}
