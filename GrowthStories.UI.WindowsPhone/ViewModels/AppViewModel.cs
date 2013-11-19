
using ReactiveUI;
using ReactiveUI.Mobile;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using Ninject;
using Growthstories.UI.WindowsPhone;
using Growthstories.UI.ViewModel;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain;
using System.Threading.Tasks;
using Growthstories.Domain.Messaging;
using Growthstories.UI.WindowsPhone.ViewModels;
using EventStore.Persistence;
using EventStore;


namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public sealed class AppViewModel : Growthstories.UI.ViewModel.AppViewModel, IApplicationRootState
    {



        private Microsoft.Phone.Controls.SupportedPageOrientation _ClientSupportedOrientations;
        public Microsoft.Phone.Controls.SupportedPageOrientation ClientSupportedOrientations
        {
            get
            {
                return _ClientSupportedOrientations;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ClientSupportedOrientations, value);
            }
        }



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
            this.Bus = Kernel.Get<IMessageBus>();

            Resolver.RegisterLazySingleton(() => new MainView(), typeof(IViewFor<MainViewModel>));
            //Resolver.RegisterLazySingleton(() => new GardenViewPage(), typeof(IViewFor<GardenViewModel>));
            //Resolver.RegisterLazySingleton(() => new PlantView(), typeof(IViewFor<PlantViewModel>));
            Resolver.RegisterLazySingleton(() => new ScheduleView(), typeof(IViewFor<ScheduleViewModel>));
            Resolver.RegisterLazySingleton(() => new AddPlantView(), typeof(IViewFor<ClientAddPlantViewModel>));
            Resolver.RegisterLazySingleton(() => new PlantActionView(), typeof(IViewFor<IPlantActionViewModel>));
            Resolver.RegisterLazySingleton(() => new YAxisShitView(), typeof(IViewFor<IYAxisShitViewModel>));
            Resolver.RegisterLazySingleton(() => new ListUsersView(), typeof(IViewFor<SearchUsersViewModel>));
            Resolver.RegisterLazySingleton(() => new GardenPivotView(), typeof(IViewFor<GardenViewModel>));
            Resolver.RegisterLazySingleton(() => new PlantPhotoPivotView(), typeof(IViewFor<PlantViewModel>));
            Resolver.RegisterLazySingleton(() => new FriendsPivotView(), typeof(IViewFor<FriendsViewModel>));
            Resolver.RegisterLazySingleton(() => new ClientTestingViewModel(Kernel, this), typeof(TestingViewModel));

            this.WhenAny(x => x.SupportedOrientations, x => x.GetValue()).Subscribe(x =>
            {
                this.ClientSupportedOrientations = (Microsoft.Phone.Controls.SupportedPageOrientation)x;
            });


            Initialize();



        }

        public override IAddEditPlantViewModel AddPlantViewModelFactory(PlantState state)
        {
            return new ClientAddPlantViewModel(state, this);
        }

        public override IYAxisShitViewModel YAxisShitViewModelFactory(IPlantViewModel pvm)
        {
            return new ClientYAxisShitViewModel(pvm, this);
        }

        public override IPlantActionViewModel PlantActionViewModelFactory<T>(PlantActionState state = null)
        {
            if ((state != null && state.Type == PlantActionType.PHOTOGRAPHED) || typeof(T) == typeof(IPlantPhotographViewModel))
                return new ClientPlantPhotographViewModel(state, this);
            else
                return base.PlantActionViewModelFactory<T>(state);
        }





    }


}

