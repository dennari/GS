using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using Growthstories.Domain.Messaging;
using Growthstories.Core;
using EventStore;
using CommonDomain;

namespace Growthstories.Domain.Entities
{


    public class GSApp : AggregateBase<GSAppState, GSAppCreated>,
       ICommandHandler<CreateGSApp>,
       ICommandHandler<CreateUser>,
       ICommandHandler<CreatePlant>
    {
        public void Handle(CreateGSApp command)
        {
            RaiseEvent(new GSAppCreated(command));
        }
        public void Handle(CreatePlant command)
        {
            RaiseEvent(new SyncStreamCreated(command));
        }
        public void Handle(CreateUser command)
        {
            RaiseEvent(new SyncStreamCreated(command));
        }
    }
}
