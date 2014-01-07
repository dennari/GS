using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI
{
    public class ProjectionBase : IEventHandler, IAsyncEventHandler
    {
        public void Handle(IEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            ((dynamic)this).Handle((dynamic)@event);

        }

        public Task HandleAsync(IEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");
            return Task.Run(async () =>
            {
                await ((dynamic)this).HandleAsync((dynamic)@event);
            });
        }
    }
}
