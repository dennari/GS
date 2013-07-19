


using CommonDomain;
using CommonDomain.Core;
using EventStore;
using EventStore.Persistence;
using EventStore.Persistence.InMemoryPersistence;
using EventStore.Serialization;
using GalaSoft.MvvmLight.Messaging;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Messaging;
using Growthstories.Domain.Services;
using Growthstories.Sync;
using Growthstories.UI;
using Growthstories.UI.ViewModel;
using Ninject.Modules;
using System;

namespace GrowthStories.UI.WindowsPhone
{
    public class BootstrapDesign : NinjectModule
    {


        public override void Load()
        {
            Bind<IUserService>().To<NullUserService>().InSingletonScope();
            Bind<IUIPersistence>().To<NullUIPersistence>().InSingletonScope();
            Bind<INavigationService>().To<NavigationService>().InSingletonScope();
            Bind<IMessenger>().To<Messenger>().InSingletonScope();
            Bind<IDispatchCommands>().To<NullCommandHandler>().InSingletonScope();

            Bind<ActionProjection>().ToSelf().InSingletonScope();
            Bind<PlantProjection>().ToSelf().InSingletonScope();
            Bind<AuthTokenService>().ToSelf().InSingletonScope();

            Bind<PlantViewModel>().ToSelf().InSingletonScope().OnActivation((ctx, vm) =>
            {
                vm.Handle(new ShowPlantView(new PlantCreated(Guid.NewGuid(), "Jore", Guid.NewGuid())));
                vm.Actions.Add(new Commented(Guid.NewGuid(), Guid.NewGuid(), "Test Note!") { Created = DateTimeOffset.Now });
                vm.Actions.Add(new Watered(Guid.NewGuid(), Guid.NewGuid(), "Test Note!") { Created = DateTimeOffset.Now });

            });

            Bind<GardenViewModel>().ToSelf().InSingletonScope().OnActivation((ctx, vm) =>
            {
                vm.Plants.Add(new PlantCreated(Guid.NewGuid(), "Jore", Guid.NewGuid()));
                vm.Plants.Add(new PlantCreated(Guid.NewGuid(), "Kari", Guid.NewGuid()));
            });


        }

    }

}


