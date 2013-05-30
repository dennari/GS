﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Core
{
    public interface IEvent : IMessage
    {
        Guid EventId { get; set; }
        Guid EntityId { get; set; }
        int EntityVersion { get; set; }
    }
}
