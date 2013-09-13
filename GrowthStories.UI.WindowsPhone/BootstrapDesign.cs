


using CommonDomain;
using CommonDomain.Core;
using EventStore;
using EventStore.Persistence;
using EventStore.Persistence.InMemoryPersistence;
using EventStore.Serialization;
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
            Bind<IDispatchCommands>().To<NullCommandHandler>().InSingletonScope();

            Bind<AuthTokenService>().ToSelf().InSingletonScope();

        }

    }

}


