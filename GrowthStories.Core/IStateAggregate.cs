﻿using CommonDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Core
{
    public interface ICreatableAggregate : IAggregate
    {
        IAggregate Create(IMemento state);
    }
}
