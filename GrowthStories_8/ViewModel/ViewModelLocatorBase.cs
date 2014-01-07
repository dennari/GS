using GalaSoft.MvvmLight;
using Growthstories.WP8.Domain.Entities;
using Ninject;

namespace Growthstories.WP8.ViewModel
{
    public abstract class ViewModelLocatorBase
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


        public ViewModelLocatorBase(IKernel kernel)
        {

            _kernel = kernel;
            bind();


            //SimpleIoc.Default.Register<INavigationService, NavigationService>();
            //SimpleIoc.Default.Register<MainViewModel>();
        }

        protected abstract void bind();

    }
}