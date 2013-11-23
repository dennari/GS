
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
            Resolver.RegisterLazySingleton(() => new ScheduleView(), typeof(IViewFor<IScheduleViewModel>));
            Resolver.RegisterLazySingleton(() => new SignInRegisterView(), typeof(IViewFor<ISignInRegisterViewModel>));
            Resolver.RegisterLazySingleton(() => new AddPlantView(), typeof(IViewFor<IAddEditPlantViewModel>));
            Resolver.RegisterLazySingleton(() => new PlantActionView(), typeof(IViewFor<IPlantActionViewModel>));
            //Resolver.RegisterLazySingleton(() => new TimelineActionView(), typeof(IViewFor<ITimelineActionViewModel>));
            Resolver.RegisterLazySingleton(() => new YAxisShitView(), typeof(IViewFor<IYAxisShitViewModel>));
            Resolver.RegisterLazySingleton(() => new ListUsersView(), typeof(IViewFor<ISearchUsersViewModel>));
            Resolver.RegisterLazySingleton(() => new PlantActionListView(), typeof(IViewFor<IPlantActionListViewModel>));
            Resolver.RegisterLazySingleton(() => new GardenPivotView(), typeof(IViewFor<IGardenPivotViewModel>));
            Resolver.RegisterLazySingleton(() => new PlantPhotoPivotView(), typeof(IViewFor<IPlantViewModel>));
            Resolver.RegisterLazySingleton(() => new FriendsPivotView(), typeof(IViewFor<IFriendsViewModel>));

            Resolver.RegisterLazySingleton(() => new ClientTestingViewModel(Kernel, this), typeof(TestingViewModel));
            Resolver.Register(() => new ClientAddEditPlantViewModel(this), typeof(IAddEditPlantViewModel));
            //Resolver.Re
            this.WhenAny(x => x.SupportedOrientations, x => x.GetValue()).Subscribe(x =>
            {
                this.ClientSupportedOrientations = (Microsoft.Phone.Controls.SupportedPageOrientation)x;
            });

            ViewLocator.ViewModelToViewModelInterfaceFunc = T =>
            {
                if (T is IAddEditPlantViewModel)
                    return typeof(IAddEditPlantViewModel);
                //if (T is ITimelineActionViewModel)
                //    return typeof(ITimelineActionViewModel);
                if (T is ISignInRegisterViewModel)
                    return typeof(ISignInRegisterViewModel);
                if (T is IPlantActionViewModel)
                    return typeof(IPlantActionViewModel);
                if (T is IYAxisShitViewModel)
                    return typeof(IYAxisShitViewModel);
                if (T is IScheduleViewModel)
                    return typeof(IScheduleViewModel);
                if (T is ISearchUsersViewModel)
                    return typeof(ISearchUsersViewModel);
                if (T is IGardenPivotViewModel)
                    return typeof(IGardenPivotViewModel);
                if (T is IGardenViewModel)
                    return typeof(IGardenViewModel);
                if (T is IPlantViewModel)
                    return typeof(IPlantViewModel);
                if (T is IFriendsViewModel)
                    return typeof(IFriendsViewModel);
                if (T is IPlantActionListViewModel)
                    return typeof(IPlantActionListViewModel);
                return T.GetType();

            };
            Initialize();



        }

        public override IAddEditPlantViewModel EditPlantViewModelFactory(IPlantViewModel pvm)
        {
            return new ClientAddEditPlantViewModel(this, pvm);
        }

        public override IYAxisShitViewModel YAxisShitViewModelFactory(IPlantViewModel pvm)
        {
            return new ClientYAxisShitViewModel(pvm, this);
        }

        public override IPlantActionViewModel PlantActionViewModelFactory(PlantActionType type, PlantActionState state = null)
        {
            if (type == PlantActionType.PHOTOGRAPHED)
                return new ClientPlantPhotographViewModel(this, state);
            else
                return base.PlantActionViewModelFactory(type, state);
        }





    }


}

