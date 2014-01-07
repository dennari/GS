using CommonDomain;
using CommonDomain.Persistence;
using EventStore;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;

namespace Growthstories.Sync
{
    public static class StoreExtensions
    {
        public static string ToString(this EventMessage em)
        {
            return em.Body.ToString();
        }



    }
}
