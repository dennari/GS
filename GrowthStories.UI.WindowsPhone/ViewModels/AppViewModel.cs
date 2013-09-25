
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


    public class AppViewModel : Growthstories.UI.ViewModel.AppViewModel, IApplicationRootState
    {



        protected Microsoft.Phone.Controls.SupportedPageOrientation _ClientSupportedOrientations;
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
            Resolver.RegisterLazySingleton(() => new PlantView(), typeof(IViewFor<PlantViewModel>));
            Resolver.RegisterLazySingleton(() => new ScheduleView(), typeof(IViewFor<ScheduleViewModel>));
            Resolver.RegisterLazySingleton(() => new AddPlantView(), typeof(IViewFor<ClientAddPlantViewModel>));
            //Resolver.RegisterLazySingleton(() => new EditPlantView(), typeof(IViewFor<ClientEditPlantViewModel>));
            Resolver.RegisterLazySingleton(() => new AddWaterView(), typeof(IViewFor<PlantWaterViewModel>));
            Resolver.RegisterLazySingleton(() => new AddCommentView(), typeof(IViewFor<PlantCommentViewModel>));
            Resolver.RegisterLazySingleton(() => new AddFertilizerView(), typeof(IViewFor<PlantFertilizeViewModel>));
            Resolver.RegisterLazySingleton(() => new AddMeasurementView(), typeof(IViewFor<PlantMeasureViewModel>));
            Resolver.RegisterLazySingleton(() => new AddPhotographView(), typeof(IViewFor<PlantPhotographViewModel>));
            Resolver.RegisterLazySingleton(() => new YAxisShitView(), typeof(IViewFor<YAxisShitViewModel>));


            this.WhenAny(x => x.SupportedOrientations, x => x.GetValue()).Subscribe(x => this.ClientSupportedOrientations = (Microsoft.Phone.Controls.SupportedPageOrientation)x);





        }

        public override AddPlantViewModel AddPlantViewModelFactory(PlantState state)
        {
            return new ClientAddPlantViewModel(state, this);
        }

        public override IPlantActionViewModel PlantActionViewModelFactory<T>(ActionBase state = null)
        {
            return ClientPlantActionViewModel.Create<T>(state, this);
        }

        //public override IPlantViewModel PlantFactory(Guid id, IGardenViewModel garden)
        //{
        //    return new ClientPlantViewModel(
        //       ((Plant)Kernel.Get<IGSRepository>().GetById(id)).State,
        //       garden,
        //       this
        //   );
        //}



        public override IObservable<IPlantActionViewModel> PlantActionViewModelFactory(PlantState state)
        {


            Func<Guid, IGSAggregate> f = Kernel.Get<IGSRepository>().GetById;

            var af = f.ToAsync(RxApp.TaskpoolScheduler);

            return af(state.UserId)
                .OfType<User>()
                .Select(x => x.State.Actions.Where(y => y.PlantId == state.Id).ToObservable())
                .Switch()
                .Select(x => ClientPlantActionViewModel.Create(x, this));



        }



        public override async Task AddTestData()
        {
            await Task.Run(async () =>
            {
                await base.AddTestData();
                var Ctx = Kernel.Get<IUserService>().CurrentUser;
                // TEST DATA   
                AddPlant(new CreatePlant(Guid.NewGuid(), "Jore", Ctx.Id)
                {
                    Profilepicture = new Photo()
                    {
                        LocalUri = "/TestData/517e100d782a828894.jpg"
                    }
                });
                AddPlant(new CreatePlant(Guid.NewGuid(), "Jari", Ctx.Id)
                {
                    Profilepicture = new Photo()
                    {
                        LocalUri = "/TestData/flowers-from-the-conservatory.jpg"
                    }
                });
            });
        }

        public override async Task ClearDB()
        {
            await Task.Run(async () =>
            {
                await base.ClearDB();
                var db = Kernel.Get<IPersistSyncStreams>();
                db.Purge();
                Kernel.Get<IGSRepository>().ClearCaches();
                Kernel.Get<OptimisticPipelineHook>().Dispose();
                ((FakeUserService)Kernel.Get<IUserService>()).EnsureCurrenUser();

            });
        }

        private void AddPlant(CreatePlant cmd)
        {
            var Factory = Kernel.Get<IAggregateFactory>();
            var Store = Kernel.Get<IGSRepository>();
            var Ctx = Kernel.Get<IUserService>().CurrentUser;

            var p1 = Factory.Build<Plant>();
            p1.Handle(cmd);
            //p1.Handle(new AddWateringAction(cmd.EntityId));
            //p1.Handle(new AddFertilizingAction(cmd.EntityId));

            //p1.Handle();

            Store.Save(p1);

            var g = (Garden)Store.GetById(Ctx.GardenId);
            g.Handle(new AddPlant(Ctx.GardenId, p1.State.Id, p1.State.Name));
            Store.Save(g);

            var u = (User)Store.GetById(Ctx.Id);
            u.Handle(new Water(Ctx.Id, p1.State.Id, "NOTE"));
            u.Handle(new Fertilize(Ctx.Id, p1.State.Id, "NOTE"));
            u.Handle(new Comment(Ctx.Id, p1.State.Id, "NOTE") { Created = DateTimeOffset.Now });
            // u.Handle(new Photograph(Ctx.Id, p1.State.Id, "My baby!", "/TestData/517e100d782a828894.jpg"));


            Store.Save(u);




        }



    }


}

