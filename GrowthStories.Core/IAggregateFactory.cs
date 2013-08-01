using CommonDomain;
using CommonDomain.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Core
{
    public interface IAggregateFactory
    {

        T Build<T>(IMemento state) where T : IGSAggregate, new();
        T Build<T>() where T : IGSAggregate, new();

        IGSAggregate Build(Type type);
    }
}
