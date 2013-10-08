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
            return new UserCreated(CreateUser())
            {
                AggregateVersion = 1,
                MessageId = FakeEventFactory.FakeEventId,
                Created = FakeEventFactory.FakeCreated
            };
        }

        protected CreateUser CreateUser()
        {
            return new CreateUser(id, "dennari", "swordfish", "email@net.com");

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
