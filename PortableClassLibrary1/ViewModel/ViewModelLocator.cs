using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Growthstories.PCL.Models;
using Growthstories.PCL.Services;
using Ninject;

namespace Growthstories.PCL.ViewModel
{
    public abstract class ViewModelLocator
    {
        protected IKernel _kernel;


        public GardenViewModel Garden
        {
            get
            {

                return _kernel.Get<GardenViewModel>();
            }
        }

        public PlantViewModel Plant
        {
            get
            {

                return _kernel.Get<PlantViewModel>();
            }
        }

        public ActionViewModel PlantAction
        {
            get
            {

                return _kernel.Get<ActionViewModel>();
            }
        }


        public AddPlantViewModel AddPlant
        {
            get
            {

                return _kernel.Get<AddPlantViewModel>();
            }
        }


        public ViewModelLocator(IKernel kernel)
        {

            _kernel = kernel;
            bind();


            //SimpleIoc.Default.Register<INavigationService, NavigationService>();
            //SimpleIoc.Default.Register<MainViewModel>();
        }

        protected abstract void bind();

    }
}