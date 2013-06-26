using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Growthstories.Domain.Messaging;
using Growthstories.Core;

namespace Growthstories.DomainTests
{
    public class create_user : gs_spec
    {


        [Test]
        public void given_no_prior_history()
        {
            Given();
            When(new CreateUser(id));
            Expect(new UserCreated(id)
            {
                EntityVersion = 1,
                EventId = FakeEventFactory.FakeEventId,
                Created = FakeEventFactory.FakeCreated
            });
        }

        [Test]
        public void given_created_user()
        {
            Given(new UserCreated(id));
            When(new CreateUser(id));
            Expect("rebirth");
        }

    }
}
