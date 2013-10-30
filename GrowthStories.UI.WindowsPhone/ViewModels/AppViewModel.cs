
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
            Resolver.RegisterLazySingleton(() => new GardenViewPage(), typeof(IViewFor<GardenViewModel>));
            Resolver.RegisterLazySingleton(() => new PlantView(), typeof(IViewFor<PlantViewModel>));
            Resolver.RegisterLazySingleton(() => new ScheduleView(), typeof(IViewFor<ScheduleViewModel>));
            Resolver.RegisterLazySingleton(() => new AddPlantView(), typeof(IViewFor<ClientAddPlantViewModel>));
            //Resolver.RegisterLazySingleton(() => new EditPlantView(), typeof(IViewFor<ClientEditPlantViewModel>));
            Resolver.RegisterLazySingleton(() => new AddWaterView(), typeof(IViewFor<PlantWaterViewModel>));
            Resolver.RegisterLazySingleton(() => new AddCommentView(), typeof(IViewFor<PlantCommentViewModel>));
            Resolver.RegisterLazySingleton(() => new AddFertilizerView(), typeof(IViewFor<PlantFertilizeViewModel>));
            Resolver.RegisterLazySingleton(() => new AddMeasurementView(), typeof(IViewFor<PlantMeasureViewModel>));
            Resolver.RegisterLazySingleton(() => new AddPhotographView(), typeof(IViewFor<ClientPlantPhotographViewModel>));
            Resolver.RegisterLazySingleton(() => new YAxisShitView(), typeof(IViewFor<YAxisShitViewModel>));
            Resolver.RegisterLazySingleton(() => new ListUsersView(), typeof(IViewFor<ListUsersViewModel>));
            Resolver.RegisterLazySingleton(() => new GardenPivotView(), typeof(IViewFor<GardenPivotViewModel>));

            Resolver.RegisterLazySingleton(() => new ClientTestingViewModel(Kernel, this), typeof(ITestingViewModel));

            this.WhenAny(x => x.SupportedOrientations, x => x.GetValue()).Subscribe(x => this.ClientSupportedOrientations = (Microsoft.Phone.Controls.SupportedPageOrientation)x);


            this._Model = Initialize(Kernel.Get<IGSRepository>());



        }

        public override AddPlantViewModel AddPlantViewModelFactory(PlantState state)
        {
            return new ClientAddPlantViewModel(state, this);
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

