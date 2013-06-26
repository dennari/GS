using CommonDomain;
using Growthstories.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public class FakeAncestorFactory : IAncestorFactory
    {
        private readonly User u = new User();
        public static Guid FakeUserId = Guid.Parse("10000000-0000-0000-0000-000000000000");

        public FakeAncestorFactory()
        {
            u.Create(FakeUserId);
        }

        public IMemento GetAncestor()
        {
            return u;
        }
    }
}
