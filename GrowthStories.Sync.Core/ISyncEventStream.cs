﻿using EventStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface ISyncEventStream : IEntityEventStream, IEventStream, IEquatable<ISyncEventStream>
    {
        void Rebase(ISyncEventStream remoteStream);
    }
}
