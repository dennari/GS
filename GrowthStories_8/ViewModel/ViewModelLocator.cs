using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Ninject;
using Growthstories.PCL.ViewModel;

namespace Growthstories.WP8.ViewModel
{
    public class ViewModelLocator : Growthstories.PCL.ViewModel.ViewModelLocator
    {
        private static IKernel GetKernel()
        {
            return new StandardKernel();
        }
        
        public ViewModelLocator() : base(GetKernel())
        {
        }

    }
}