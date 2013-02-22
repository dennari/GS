using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Growthstories.PCL.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Growthstories.Tests
{
    [TestClass]
    public class GrowthstoriesTest
    {
        [TestMethod]
        public void Garden_lists_Plants()
        {

            User gardener = new User();
            Garden garden = new Garden(gardener);
            Plant plant1 = new Plant();
            Plant plant2 = new Plant();

            IList<Plant> plants = garden.Plants;
            garden.Plants.Add(plant1);
            garden.Plants.Add(plant2);

            Assert.AreSame(plants[0], plant1);
            Assert.AreSame(plants[1], plant2);
            Assert.AreSame(gardener, garden.Owner);
        }

    }
}

