using CommonDomain;
using CommonDomain.Persistence;
using System;
using System.Reflection;
using Growthstories.Core;

namespace Growthstories.Domain.Services
{





    public class AggregateFactory : IAggregateFactory
    {
        private IEventFactory eFactory;
        //private readonly IKernel Container;


        public AggregateFactory(IEventFactory eFactory)
        {
            this.eFactory = eFactory;
        }


        public T Build<T>() where T : IGSAggregate, new()
        {
            var instance = new T();
            instance.SetEventFactory(eFactory);
            return instance;
        }

        public T Build<T>(IMemento state) where T : IGSAggregate, new()
        {
            if (state.Id == default(Guid) || state.Version == 0)
            {
                throw new ArgumentException();
            }
            var instance = Build<T>();
            instance.ApplyState(state);
            return instance;
        }

    }
}
