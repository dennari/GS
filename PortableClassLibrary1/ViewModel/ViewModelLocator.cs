using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Ninject;

namespace Growthstories.PCL.ViewModel
{
    public class ViewModelLocator
    {
        private IKernel _kernel;
        
        
        public GalleryViewModel Gallery
        {
            get
            {
                return _kernel.Get<GalleryViewModel>();
            }
        }


        public ViewModelLocator(IKernel kernel)
        {
           
            _kernel = kernel;
            bind();
            //SimpleIoc.Default.Register<INavigationService, NavigationService>();
            //SimpleIoc.Default.Register<MainViewModel>();
        }

        private void bind()
        {
            _kernel.Bind<GalleryViewModel>().ToSelf().InSingletonScope();

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // SimpleIoc.Default.Register<IRssService, DesignRssService>();
            }
            else
            {
                //SimpleIoc.Default.Register<IRssService, RssService>();
            }
        }

    }
}