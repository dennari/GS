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


using EventStore;
using EventStore.Logging;
using EventStore.Persistence;
using EventStore.Persistence.SqlPersistence;
using EventStore.Serialization;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Domain.Services;
using Growthstories.Sync;
using Growthstories.UI;
using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.ViewModels;
using Ninject;
using Ninject.Parameters;
using ReactiveUI;
using SQLite;
using System.IO;
using Windows.Storage;

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
        private AddPlantViewModel _AddPlantVM;
        private MainViewModel _MainVM;
        private bool DebugDesignSwitch = false;
        public ViewModelLocator()
        {

            if (DesignModeDetector.IsInDesignMode() || DebugDesignSwitch)
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

        public MainViewModel MainVM
        {
            get
            {
                if (DesignModeDetector.IsInDesignMode() || DebugDesignSwitch)
                {
                    return new MainViewModel(new NullUserService(), new MessageBus(), new NavigationService());
                }
                else
                {

                    return _MainVM == null ? _MainVM = Kernel.Get<MainViewModel>() : _MainVM;
                }
            }
        }

        public PlantViewModel PlantVM
        {
            get
            {
                return _PlantVM == null ? _PlantVM = Kernel.Get<PlantViewModel>() : _PlantVM;
            }
        }

        public AddPlantViewModel AddPlantVM
        {
            get
            {
                return _AddPlantVM == null ? _AddPlantVM = Kernel.Get<ClientAddPlantViewModel>() : _AddPlantVM;
            }
        }


        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }


    }
}