using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public class SynchronizerCommandHandler : IRegisterHandlers
    {
        private readonly ISynchronizerService _service;
        public SynchronizerCommandHandler(ISynchronizerService service)
        {
            this._service = service;
        }

        public async Task<object> Synchronize(IGSAggregate agg, IEntityCommand command)
        {
            var Agg = (Synchronizer)agg;
            return await Agg.Handle((Synchronize)command, _service);
        }

        public IDictionary<Tuple<Type, Type>, Action<IGSAggregate, IEntityCommand>> RegisterHandlers()
        {
            return null;
        }

        public IDictionary<Tuple<Type, Type>, Func<IGSAggregate, IEntityCommand, Task<object>>> RegisterAsyncHandlers()
        {
            return new Dictionary<Tuple<Type, Type>, Func<IGSAggregate, IEntityCommand, Task<object>>>()
            {
                {Tuple.Create(typeof(Synchronizer),typeof(Synchronize)),this.Synchronize}
            };
        }
    }
}
