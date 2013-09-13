
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
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain;
using System.Threading.Tasks;
using Growthstories.Domain.Messaging;

namespace Growthstories.UI.WindowsPhone.ViewModels
{

    public class AppViewModel : Growthstories.UI.ViewModel.AppViewModel, IApplicationRootState
    {





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

            var Ctx = Kernel.Get<IUserService>().CurrentUser;

            // TEST DATA   
            AddPlant(new CreatePlant(Guid.NewGuid(), "Jore", Ctx.Id));
            AddPlant(new CreatePlant(Guid.NewGuid(), "Jari", Ctx.Id));

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

            Store.Save(u);




        }



    }


}

