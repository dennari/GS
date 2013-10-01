using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface IEndpoint
    {
        string Name { get; }
        Uri PullUri { get; }
        Uri PushUri { get; }
        Uri AuthUri { get; }
        Uri ClearDBUri { get; }

    }
}
