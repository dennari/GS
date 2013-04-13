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
using Growthstories.PCL.Helpers;
using Growthstories.WP8.Helpers;
using Growthstories.WP8.Services;
using M8 = Growthstories.WP8.Models;
using System.Diagnostics;
using System.Linq;


namespace Growthstories.Tests
{



    [TestClass]
    public class GrowthstoriesTest
    {

        public IKernel kernel;
        private const string TestPhotoPath = "/Assets/rose.jpg";

        [TestInitialize]
        public void TestInitialize()
        {
            this.kernel = new StandardKernel();
            var k = this.kernel;

            k.Bind<GardenViewModel>().ToSelf().InSingletonScope();
            k.Bind<AddPlantViewModel>().ToSelf().InSingletonScope();
            k.Bind<ActionViewModel>().ToSelf().InSingletonScope();
            k.Bind<PlantViewModel>().ToSelf().InSingletonScope();

            k.Bind<User>().ToSelf().InSingletonScope();
            k.Bind<Garden>().ToSelf().InSingletonScope().Named("My");
            k.Bind<Plant>().ToSelf();
            k.Bind<IPlantDataService>().To<FakePlantDataService>().InSingletonScope();
            k.Bind<IPictureService>().To<PictureService>().InSingletonScope();
            k.Bind<INavigationService>().To<FakeNavigationService>().InSingletonScope();
            k.Bind<IDataService>().To<FakeWP8DataService>().InSingletonScope();

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
            var g = new M8.Garden();
            g.Plants.Add(new M8.Plant()
            {
                Name = "Sepi",
                Genus = "Aloe Vera"
            });
            g.Plants.Add(new M8.Plant()
            {
                Name = "Jore",
                Genus = "Aloe Maximus"
            });

            using (var db = new MyDataContext())
            {
                db.DeleteDatabase();
                if (db.DatabaseExists() == false)
                {
                    // Create the local database.
                    db.CreateDatabase();
                    Debug.WriteLine("database deleted and created");

                }


                db.Gardens.InsertOnSubmit(g);
                db.Plants.InsertAllOnSubmit(g.Plants);
                db.SubmitChanges();

            }

            Assert.IsNotNull(g.Id);
            foreach (var item in g.Plants)
            {
                Assert.IsNotNull(item.Id);
            }

            using (var db = new MyDataContext())
            {
                Assert.AreEqual(db.Gardens.Count(), 1);
                g = db.Gardens.First();
                Assert.AreEqual(g.Plants.Count(), 2);

            }

            foreach (var p in g.Plants)
            {
                Debug.WriteLine("{0}, {1}, {2}", p.Name, p.Genus, p.Id);
                Assert.AreSame(g, p.Garden);
            }



        }




    }
}

