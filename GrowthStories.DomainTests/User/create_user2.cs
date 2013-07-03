using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Growthstories.Domain.Messaging;
using Growthstories.Core;

namespace Growthstories.DomainTests
{
    public class create_user : gs_spec
    {
        protected UserCreated UserCreated()
        {
            return new UserCreated()
            {
                EntityId = id,
                EntityVersion = 1,
                EventId = FakeEventFactory.FakeEventId,
                Created = FakeEventFactory.FakeCreated,
                Username = "dennari",
                Password = "swordfish"
            };
        }

        protected CreateUser CreateUser()
        {
            return new CreateUser(id, "dennari", "swordfish", "dennari@me.net");

        }

        [Test]
        public void given_no_prior_history()
        {
            Given();
            When(CreateUser());
            Expect(UserCreated());
        }

        [Test]
        public void given_created_user()
        {
            Given(UserCreated());
            When(CreateUser());
            Expect("rebirth");
        }

    }
}
