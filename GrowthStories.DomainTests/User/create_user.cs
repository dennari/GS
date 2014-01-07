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
    public class create_user : spec_syntax<UserId>
    {
        static UserId id = new UserId(Guid.Empty);
        static IKernel kernel;
        static Boolean binded = false;

        [Test]
        public void given_no_prior_history()
        {
            Given();
            When(new CreateUser(id));
            Expect(new UserCreated(id));
        }

        [Test]
        public void given_created_user()
        {
            Given(new UserCreated(id));
            When(new CreateUser(id));
            Expect("rebirth");
        }

        protected override void ExecuteCommand(IEventStore store, ICommand<UserId> cmd)
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