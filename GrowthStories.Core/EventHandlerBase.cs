using System;
using System.Collections.Generic;
using CommonDomain;
using CommonDomain.Core;
using Growthstories.Core;
using System.Reflection;
using EventStore.Logging;
using System.Threading.Tasks;

namespace Growthstories.Core
{

    //public abstract class EventHandlerBase : IEventHandler, IAsyncEventHandler
    //{

    //    public event EventHandler<EventHandledArgs> EventHandled;

    //    public void Handle(IEvent @event)
    //    {
    //        if (@event == null)
    //            throw new ArgumentNullException("event");
    //        ((dynamic)this).Handle((dynamic)@event);
    //        if (this.EventHandled != null)
    //            this.EventHandled(this, new EventHandledArgs(@event));
    //    }

    //    public Task HandleAsync(IEvent @event)
    //    {
    //        if (@event == null)
    //            throw new ArgumentNullException("event");
    //        return Task.Run(async () =>
    //        {
    //            await ((dynamic)this).HandleAsync((dynamic)@event);
    //            if (this.EventHandled != null)
    //                this.EventHandled(this, new EventHandledArgs(@event));
    //        });
    //    }

    //}

    //public class EventHandledArgs : EventArgs
    //{
    //    public IEvent @event { get; private set; }

    //    public EventHandledArgs(IEvent e)
    //    {
    //        this.@event = e;
    //    }
    //}

}
