using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Ninject;
using Growthstories.PCL.ViewModel;
using Growthstories.PCL.Models;
using Growthstories.PCL.Services;
using Growthstories.WP8.Services;
using Growthstories.PCL.Helpers;
using Growthstories.WP8.Helpers;
using System;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;
using GalaSoft.MvvmLight.Command;
using System.Windows.Resources;
using System.Windows;

namespace Growthstories.WP8.ViewModel
{
    public class ViewModelLocator : Growthstories.PCL.ViewModel.ViewModelLocator
    {
        private static IKernel GetKernel()
        {
            return new StandardKernel();
        }

        public ViewModelLocator()
            : base(GetKernel())
        {
        }


        protected async override void bind()
        {
            var k = _kernel;
            k.Bind<GardenViewModel>().ToSelf().InSingletonScope();
            k.Bind<AddPlantViewModel>().ToSelf().InSingletonScope();
            k.Bind<ActionViewModel>().ToSelf().InSingletonScope();
            k.Bind<PlantViewModel>().ToSelf().InSingletonScope();

            k.Bind<User>().ToSelf().InSingletonScope();
            k.Bind<Garden>().ToSelf().InSingletonScope().Named("My");
            k.Bind<Plant>().ToSelf();
            k.Bind<IPlantDataService>().To<FakePlantDataService>().InSingletonScope();
            k.Bind<IPictureService>().To<PictureService>().InSingletonScope();
            k.Bind<INavigationService>().To<NavigationService>().InSingletonScope();

            this.Garden.PropertyChanged += this.Plant.SelectedPlantChanged;
            this.Plant.PropertyChanged += this.PlantAction.SelectedActionChanged;


            if (ViewModelBase.IsInDesignModeStatic)
            {

            }
            else
            {
                //SimpleIoc.Default.Register<IRssService, RssService>();
            }
        }

        public RelayCommand LoadTestData()
        {
            return new RelayCommand(async () =>
            {
                var k = _kernel;
                var gvm = this.Garden;
                gvm.MyGarden = k.Get<Garden>();
                gvm.MyGarden.Plants.Add(new Plant(k.Get<IPlantDataService>())
                {
                    Genus = "Aloe Vera",
                    Name = "Seppo",
                    ProfilePicturePath = "/Assets/rose.jpg"
                });
                var p0 = gvm.MyGarden.Plants[0];
                var uri = new Uri("Assets/rose.jpg", UriKind.Relative);
                // var file = await StorageFile.GetFileFromApplicationUriAsync);
                StreamResourceInfo sri = Application.GetResourceStream(uri);
                p0.Actions.Add(new PhotoAction(p0, sri.Stream));
                var a0 = p0.Actions[0];
                a0.CreatedAt = a0.CreatedAt.Value.AddDays(-2);
                a0.ModifiedAt = a0.CreatedAt;

                p0.Actions.Add(new WateringAction(p0));
                a0 = p0.Actions[1];
                a0.CreatedAt = a0.CreatedAt.Value.AddDays(-4);
                a0.ModifiedAt = a0.CreatedAt;

                gvm.MyGarden.Plants.Add(new Plant(k.Get<IPlantDataService>())
                {
                    Genus = "Phytoliforus",
                    Name = "Kipa",
                    ProfilePicturePath = "/Assets/ApplicationIcon.png"
                });
                gvm.MyGarden.Plants.Add(new Plant(k.Get<IPlantDataService>())
                {
                    Name = "Jare",
                    Genus = "Orchidealissimo",
                    ProfilePicturePath = "/Assets/AppBar/add.png"
                });
                this.Plant.CurrentPlant = gvm.MyGarden.Plants[0];
            });
        }



    }
}