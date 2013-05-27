using CommonDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrowthStories.Core
{
    public interface ICreatableAggregate : IAggregate
    {
        IAggregate Create(IMemento state);
    }
}
