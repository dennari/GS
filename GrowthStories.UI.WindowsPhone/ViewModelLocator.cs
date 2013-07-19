/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:GrowthStories.UI.WindowsPhone"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Growthstories.UI;
using Growthstories.UI.ViewModel;
using Ninject;

namespace GrowthStories.UI.WindowsPhone
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        private readonly IKernel Kernel;
        private GardenViewModel _GardenVM;
        private PlantViewModel _PlantVM;

        public ViewModelLocator()
        {

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
                this.Kernel = new StandardKernel(new BootstrapDesign());
            }
            else
            {
                // Create run time view services and models
                this.Kernel = new StandardKernel(new Bootstrap());
            }

            //this.Bootstrap();

        }

        public GardenViewModel GardenVM
        {
            get
            {
                return _GardenVM == null ? _GardenVM = Kernel.Get<GardenViewModel>() : _GardenVM;
            }
        }

        public PlantViewModel PlantVM
        {
            get
            {
                return _PlantVM == null ? _PlantVM = Kernel.Get<PlantViewModel>() : _PlantVM;
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}