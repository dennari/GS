using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.UI
{
    public interface IHandles<T>
    {
        void Handle(T @event);
    }
}
