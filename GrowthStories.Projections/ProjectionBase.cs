using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Projections
{
    public class ProjectionBase : IEventHandler, IAsyncEventHandler
    {
        public void Handle(IEvent @event)
        {
            ((dynamic)this).Handle((dynamic)@event);

        }

        public Task HandleAsync(IEvent @event)
        {
            return Task.Run(async () =>
            {
                await ((dynamic)this).HandleAsync((dynamic)@event);
            });
        }
    }
}
