using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Ninject;
using Growthstories.PCL.ViewModel;
using Growthstories.WP8.Models;
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
            k.Bind<IDataService>().To<FakeDataService>().InSingletonScope();
            k.Bind<IPictureService>().To<PictureService>().InSingletonScope();
            k.Bind<INavigationService>().To<NavigationService>().InSingletonScope();

            this.Garden.PropertyChanged += this.Plant.SelectedPlantChanged;
            this.Plant.PropertyChanged += this.PlantAction.SelectedActionChanged;

            // load test stuff
            this.Garden.MyGarden = await k.Get<IDataService>().LoadGarden(k.Get<User>());

            if (ViewModelBase.IsInDesignModeStatic)
            {
                this.Garden.SelectedPlant = this.Garden.MyGarden.Plants[1];
                this.Plant.SelectedAction = this.Plant.CurrentPlant.Actions[0];
            }
            else
            {
                //SimpleIoc.Default.Register<IRssService, RssService>();
            }
        }



    }
}