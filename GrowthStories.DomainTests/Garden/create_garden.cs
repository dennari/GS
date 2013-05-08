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
    public class create_garden : spec_syntax<GardenId>
    {
        static GardenId id = new GardenId(Guid.Empty);
        static IKernel kernel;
        static Boolean binded = false;

        [Test]
        public void given_no_prior_history()
        {
            Given();
            When(new CreateGarden(id));
            Expect(new GardenCreated(id));
        }

        [Test]
        public void given_created_garden()
        {
            Given(new GardenCreated(id));
            When(new CreateGarden(id));
            Expect("rebirth");
        }

        protected override void ExecuteCommand(IEventStore store, ICommand<GardenId> cmd)
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