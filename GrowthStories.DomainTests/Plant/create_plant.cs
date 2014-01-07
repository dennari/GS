using Growthstories.Domain.Entities;
using Growthstories.Domain.Interfaces;
using Growthstories.Domain.Messaging;
using Ninject;
using Ninject.Parameters;
using NUnit.Framework;
using SimpleTesting;
using System;
using System.Collections.Generic;

namespace GrowthStories.DomainTests
{
    public class create_plant : spec_syntax<PlantId>
    {
        static PlantId id = new PlantId(Guid.Empty);
        static IKernel kernel;
        static Boolean binded = false;

        [Test]
        public void given_no_prior_history()
        {
            Given();
            When(new CreatePlant(id));
            Expect(new PlantCreated(id));
        }

        [Test]
        public void given_created_plant()
        {
            Given(new PlantCreated(id));
            When(new CreatePlant(id));
            Expect("rebirth");
        }

        protected override void ExecuteCommand(IEventStore store, ICommand<PlantId> cmd)
        {
            kernel.Get<ICommandHandler<IIdentity>>(new ConstructorArgument("store", store)).Execute(cmd);
            //kernel.Get<ICommandHandler<IIdentity>>().Execute(cmd);
        }

        protected override void SetupServices()
        {
            BindServices();
        }

        protected override IEventStore GetEventStore()
        {
            return kernel.Get<IEventStore>();
        }

        static void BindServices()
        {
            if (binded)
            {
                return;
            }

            kernel = new StandardKernel();
            kernel.Bind<IEventStore>().To<InMemoryStore>();
            kernel.Bind<ICommandHandler<IIdentity>>().To<CommandHandler>();
        }

    }
}