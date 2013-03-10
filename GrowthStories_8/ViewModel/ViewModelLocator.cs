using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Ninject;
using Growthstories.PCL.ViewModel;
using Growthstories.PCL.Models;
using Growthstories.PCL.Services;
using Growthstories.WP8.Services;
using Growthstories.PCL.Helpers;
using Growthstories.WP8.Helpers;

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


        protected override void bind()
        {
            _kernel.Bind<GardenViewModel>().ToSelf().InSingletonScope();
            _kernel.Bind<AddPlantViewModel>().ToSelf().InSingletonScope();
            _kernel.Bind<PlantViewModel>().ToSelf().InSingletonScope();
            _kernel.Bind<User>().ToSelf().InSingletonScope();
            _kernel.Bind<Garden>().ToSelf().InSingletonScope().Named("My");
            _kernel.Bind<Plant>().ToSelf();
            _kernel.Bind<IPlantDataService>().To<FakePlantDataService>().InSingletonScope();
            _kernel.Bind<IPictureService>().To<PictureService>().InSingletonScope();
            _kernel.Bind<INavigationService>().To<NavigationService>().InSingletonScope();


            if (false)//(ViewModelBase.IsInDesignModeStatic)
            {
                GardenViewModel gvm = _kernel.Get<GardenViewModel>();
                gvm.MyGarden = _kernel.Get<Garden>();
                gvm.MyGarden.Plants.Add(new Plant(_kernel.Get<IPlantDataService>())
                {
                    Genus = "Aloe Vera",
                    Name = "Sepi",
                    ProfilePicturePath = "/Assets/AlignmentGrid.png"
                });
                gvm.MyGarden.Plants.Add(new Plant(_kernel.Get<IPlantDataService>())
                {
                    Genus = "Phytoliforus",
                    Name = "Kipa",
                    ProfilePicturePath = "/Assets/ApplicationIcon.png"
                });
                gvm.MyGarden.Plants.Add(new Plant(_kernel.Get<IPlantDataService>())
                {
                    Name = "Jare",
                    Genus = "Orchidealissimo",
                    ProfilePicturePath = "/Assets/AppBar/add.png"
                });

                // SimpleIoc.Default.Register<IRssService, DesignRssService>();
            }
            else
            {
                //SimpleIoc.Default.Register<IRssService, RssService>();
            }
        }


    }
}