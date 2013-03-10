using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Growthstories.PCL.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ninject;
using Growthstories.PCL.Services;
using Growthstories.PCL.ViewModel;
using GalaSoft.MvvmLight;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Growthstories.Tests
{



    [TestClass]
    public class GrowthstoriesTest
    {

        public IKernel kernel;

        [TestInitialize]
        public void TestInitialize()
        {
            this.kernel = new StandardKernel();
            this.kernel.Bind<IPlantDataService>()
                       .To<FakePlantDataService>()
                       .InSingletonScope();
            //this.kernel.Bind<FakePictureService>()
            //           .ToSelf()
            //           .InSingletonScope();
        }


        [TestMethod]
        public void Garden_lists_Plants()
        {

            User gardener = new User();
            Garden garden = new Garden(gardener);
            Plant plant1 = this.kernel.Get<Plant>();
            Plant plant2 = this.kernel.Get<Plant>();

            IList<Plant> plants = garden.Plants;
            garden.Plants.Add(plant1);
            garden.Plants.Add(plant2);

            Assert.AreSame(plants[0], plant1);
            Assert.AreSame(plants[1], plant2);
            Assert.AreSame(gardener, garden.Owner);
        }

        [TestMethod]
        public void Water_Plant()
        {
            Plant plant = this.kernel.Get<Plant>();
            PlantAction action = new WateringAction(plant);

            Assert.AreSame(plant, action.Plant);

        }

        [TestMethod]
        public void Plant_Plantdata()
        {
            Plant plant = this.kernel.Get<Plant>();
            Assert.IsNull(plant.Info);
            try
            {
                // plant.load();
            }
            catch (MissingMemberException)
            {

            }
            plant.Genus = "Aloe vera";
            plant.load();
            Assert.IsInstanceOfType(plant.Info, typeof(PlantData));


        }


        [TestMethod]
        public void Timeline_Actions()
        {
            User gardener = new User();
            Garden garden = new Garden(gardener);

            Plant plant = this.kernel.Get<Plant>();
            plant.Name = "Sepi";
            plant.Genus = "Aloe Vera";

            garden.Plants.Add(plant);
            int notified = 0;
            plant.Actions.CollectionChanged += (o, e) => notified++;
            plant.Actions.Add(new WateringAction(plant));
            plant.Actions.Add(new PhotoAction(plant));
            plant.Actions.Add(new FertilizerAction(plant));

            Assert.AreEqual(plant.Actions.Count, 3);
            Assert.AreEqual(plant.Actions.Count, notified);


        }


    }
}

