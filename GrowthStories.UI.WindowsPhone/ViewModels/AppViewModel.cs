
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
            Resolver.RegisterLazySingleton(() => new AddWaterView(), typeof(IViewFor<AddWaterViewModel>));
            Resolver.RegisterLazySingleton(() => new AddCommentView(), typeof(IViewFor<AddCommentViewModel>));
            Resolver.RegisterLazySingleton(() => new AddFertilizerView(), typeof(IViewFor<AddFertilizerViewModel>));
            Resolver.RegisterLazySingleton(() => new AddMeasurementView(), typeof(IViewFor<AddMeasurementViewModel>));
            Resolver.RegisterLazySingleton(() => new AddPhotographView(), typeof(IViewFor<ClientAddPhotographViewModel>));
            Resolver.RegisterLazySingleton(() => new YAxisShitView(), typeof(IViewFor<YAxisShitViewModel>));


            this.WhenAny(x => x.SupportedOrientations, x => x.GetValue()).Subscribe(x => this.ClientSupportedOrientations = (Microsoft.Phone.Controls.SupportedPageOrientation)x);


            var Ctx = Kernel.Get<IUserService>().CurrentUser;

            // TEST DATA   
            AddPlant(new CreatePlant(Guid.NewGuid(), "Jore", Ctx.Id)
            {
                ProfilepicturePath = "/TestData/517e100d782a828894.jpg"
            });
            AddPlant(new CreatePlant(Guid.NewGuid(), "Jari", Ctx.Id)
            {
                ProfilepicturePath = "/TestData/flowers-from-the-conservatory.jpg"
            });
        }

        public override IGSRoutableViewModel ActionViewModelFactory(Type actionT, PlantState state, IGSApp app)
        {
            if (actionT == typeof(AddWaterViewModel))
                return new AddWaterViewModel(state, app);
            if (actionT == typeof(AddCommentViewModel))
                return new AddCommentViewModel(state, app);
            if (actionT == typeof(AddFertilizerViewModel))
                return new AddFertilizerViewModel(state, app);
            if (actionT == typeof(AddMeasurementViewModel))
                return new AddMeasurementViewModel(state, app);
            if (actionT == typeof(AddPhotographViewModel))
                return new ClientAddPhotographViewModel(state, app);
            return null;
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
            u.Handle(new Photograph(Ctx.Id, p1.State.Id, "My baby!", new Uri("/TestData/517e100d782a828894.jpg", UriKind.Relative)));


            Store.Save(u);




        }



    }


}

