using ReactiveUI;
using Growthstories.Core;
using System;

namespace Growthstories.Domain
{
    public static class Mixins
    {

        public static void DebugExceptionExtended(this IFullLogger This, string message, Exception exception)
        {
            if ((int)This.Level < (int)LogLevel.Debug)
                This.Debug(String.Format("{0}: {1}", message, exception.ToStringExtended()));
        }

    }
}
