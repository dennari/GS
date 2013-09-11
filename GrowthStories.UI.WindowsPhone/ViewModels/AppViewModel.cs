
using ReactiveUI;
using ReactiveUI.Mobile;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using Ninject;
using GrowthStories.UI.WindowsPhone;
using Growthstories.UI.ViewModel;
using Growthstories.Sync;
using System;
using System.Collections.Generic;

namespace Growthstories.UI.WindowsPhone.ViewModels
{

    public class AppViewModel : Growthstories.UI.ViewModel.AppViewModel, IApplicationRootState
    {



        StandardKernel Kernel;

        public AppViewModel()
            : base()
        {
            if (DesignModeDetector.IsInDesignMode())
            {
                // Create design time view services and models
                this.Kernel = new StandardKernel(new BootstrapDesign());
            }
            else
            {
                // Create run time view services and models
                this.Kernel = new StandardKernel(new Bootstrap());
            }
            Kernel.Bind<IScreen>().ToConstant(this);
            Kernel.Bind<IRoutingState>().ToConstant(this.Router);



            Resolver.RegisterLazySingleton(() => new MainView(), typeof(IViewFor<MainViewModel>));
            Resolver.RegisterLazySingleton(() => this.Kernel.Get<IMessageBus>(), typeof(IMessageBus));
            //R/resolver.RegisterLazySingleton(() => new PlayerView(), typeof(IPlayerView));
            //R/resolver.RegisterLazySingleton(() => new SettingsView(), typeof(ISettingsView));
            //R
            Resolver.RegisterLazySingleton(() => this.Kernel.Get<MainViewModel>(), typeof(IMainViewModel));

            Resolver.Register(() => new GardenViewModel(
                Guid.NewGuid(),
                (id) => new PlantViewModel(id, this.Kernel.Get<IUserService>(), this.Kernel.Get<IMessageBus>(), this),
                this.Kernel.Get<IUserService>(),
                this.Kernel.Get<IMessageBus>(), this),
                typeof(IGardenViewModel));

            Resolver.RegisterLazySingleton(() => new NotificationsViewModel(this.Kernel.Get<IUserService>(), this.Kernel.Get<IMessageBus>(), this), typeof(INotificationsViewModel));
            Resolver.RegisterLazySingleton(() => new FriendsViewModel(this.Kernel.Get<IUserService>(), this.Kernel.Get<IMessageBus>(), this), typeof(IFriendsViewModel));
            Resolver.RegisterLazySingleton(() => new AddPlantViewModel(this.Kernel.Get<IUserService>(), this.Kernel.Get<IMessageBus>(), this), typeof(IAddPlantViewModel));



        }

    }


}

