using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Growthstories.PCL.Models;
using Growthstories.WP8.Domain.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ninject;
using Growthstories.PCL.Services;
using Growthstories.WP8.ViewModel;
using GalaSoft.MvvmLight;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Growthstories.PCL.Helpers;
using Growthstories.WP8.Helpers;
using Growthstories.WP8.Services;
using M8 = Growthstories.WP8.Domain.Entities;
using System.Diagnostics;
using System.Linq;
using Ninject.Parameters;
using System.Data.Linq;


namespace Growthstories.Tests
{



    [TestClass]
    public class GrowthstoriesTest
    {

        public IDataService kernel;
        public IDataService repo;
        private const string TestPhotoPath = "/Assets/rose.jpg";

        [TestInitialize]
        public void TestInitialize()
        {
            kernel = FakeWP8DataService.getDataService();
            repo = kernel;


        }
        [TestCleanup]
        public void TestCleanup()
        {
            FakeWP8DataService.releaseDataService();

        }


        [TestMethod]
        public void Water_Plant()
        {
            Plant plant = repo.Get<Plant>();
            PlantAction action = repo.Get<WateringAction>(new ConstructorArgument("plant", plant));

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
            plant.Actions.Add(new PhotoAction());
            plant.Actions.Add(new FertilizerAction(plant));

            Assert.AreEqual(plant.Actions.Count, 3);
            Assert.AreEqual(plant.Actions.Count, notified);


        }

        [TestMethod]
        public void NavigateToAction()
        {
            var garden = kernel.Get<Garden>();
            var nav = kernel.Get<INavigationService>() as FakeNavigationService;
            Plant plant = this.kernel.Get<Plant>();
            plant.Name = "Sepi";
            plant.Genus = "Aloe Vera";

            garden.Plants.Add(plant);
            //plant.Actions.Add(new WateringAction(plant));
            var photoUri = new Uri(TestPhotoPath, UriKind.Relative);
            var action = new PhotoAction(plant, photoUri);
            plant.Actions.Add(action);
            //plant.Actions.Add(new FertilizerAction(plant));


            var gvm = kernel.Get<GardenViewModel>();
            var pvm = kernel.Get<PlantViewModel>();
            var avm = kernel.Get<ActionViewModel>();

            Assert.IsNull(gvm.SelectedPlant);
            Assert.IsNull(pvm.CurrentPlant);
            Assert.IsNull(pvm.SelectedAction);
            Assert.IsNull(avm.CurrentAction);
            Assert.AreSame(garden, gvm.MyGarden);


            // Wiring
            gvm.PropertyChanged += pvm.SelectedPlantChanged;
            pvm.PropertyChanged += avm.SelectedActionChanged;

            gvm.NavigateToPlant.Execute(plant);
            Assert.IsTrue(nav.CurrentLocation.Equals(GardenViewModel.PlantPageUri));
            Assert.AreSame(plant, pvm.CurrentPlant);

            pvm.NavigateToAction.Execute(action);
            Assert.IsTrue(nav.CurrentLocation.Equals(ActionViewModel.ActionPageUri));
            Assert.AreSame(action, avm.CurrentAction);


        }

        [TestMethod]
        public void TestLocalDb()
        {

            // Create the database if it does not exist.
            var g = kernel.Get<Garden>();
            g.Id = 1;
            Assert.IsNull(g.ParentId);
            Plant plant = this.kernel.Get<Plant>();
            plant.Id = 2;
            plant.Name = "Sepi";
            plant.Genus = "Aloe Vera";
            g.Plants.Add(plant);
            Assert.AreSame(g, plant.Parent);
            Assert.AreEqual(g.Id, plant.ParentId);
            Assert.AreSame(plant.Parent, plant.Garden);
            plant = this.kernel.Get<Plant>();
            plant.Name = "Jore";
            plant.Genus = "Aloe Maximus";
            plant.Id = 3;
            g.Plants.Add(plant);
            Assert.AreSame(g, plant.Parent);
            Assert.AreEqual(g.Id, plant.ParentId);
            Assert.AreSame(plant.Parent, plant.Garden);

            using (var db = new LocalDataContext())
            {
                db.DeleteDatabase();
                if (db.DatabaseExists() == false)
                {
                    // Create the local database.
                    db.CreateDatabase();
                    Debug.WriteLine("database deleted and created");

                }


                db.Models.InsertOnSubmit(g);
                //db.Plants.InsertAllOnSubmit(g.Plants);
                db.SubmitChanges();

            }

            Assert.IsNotNull(g.Id);
            foreach (var item in g.Plants)
            {
                Assert.IsNotNull(item.Id);
            }

            using (var db = new LocalDataContext())
            {

                var options = new DataLoadOptions();
                options.LoadWith<Garden>(gg => gg.PlantsDb);
                db.LoadOptions = options;

                var q = from m in db.Gardens select m;
                var q2 = from m in db.Plants select m;


                Assert.AreEqual(1, q.Count());
                Assert.AreEqual(3, db.Models.Count());
                Assert.AreEqual(2, q2.Count(), "plants not inserted");
                foreach (Plant p in q2)
                {
                    Debug.WriteLine("{0}, {1}, {2}, {3}", p.Name, p.Genus, p.Id, p.ParentId);
                }
                g = q.First();

            }

            Assert.AreEqual(2, g.Plants.Count(), "plants not retrieved");

            foreach (var b in g.Plants)
            {
                var p = b as Plant;
                Debug.WriteLine("{0}, {1}, {2}", p.Name, p.Genus, p.Id);
                Assert.AreSame(g, p.Garden);
                Assert.AreEqual(g.Id, p.ParentId);
            }



        }




    }
}

