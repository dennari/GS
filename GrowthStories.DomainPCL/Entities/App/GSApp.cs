using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using Growthstories.Domain.Messaging;
using Growthstories.Core;
using EventStore;
using CommonDomain;
using Growthstories.Sync;

namespace Growthstories.Domain.Entities
{


    public class GSApp : AggregateBase<GSAppState, GSAppCreated>, IGSApp,
       ICommandHandler<CreateGSApp>,
       ICommandHandler<CreateUser>,
       ICommandHandler<CreatePlant>,
       ICommandHandler<AssignAppUser>,
       ICommandHandler<SetAuthToken>,
       ICommandHandler<CreateSyncStream>,
       ICommandHandler<BecomeFollower>,
       ICommandHandler<SetSyncStamp>
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
        public void Handle(BecomeFollower command)
        {
            RaiseEvent(new SyncStreamCreated(command));
        }
        public void Handle(CreateSyncStream command)
        {
            RaiseEvent(new SyncStreamCreated(command));
        }
        public void Handle(AssignAppUser command)
        {
            RaiseEvent(new AppUserAssigned(command));
        }
        public void Handle(SetAuthToken command)
        {
            RaiseEvent(new AuthTokenSet(command));
        }
        public void Handle(SetSyncStamp command)
        {
            RaiseEvent(new SyncStampSet(command));
        }

        IGSAppState IGSApp.State
        {
            get
            {
                return (IGSAppState)this.State;
            }
        }
    }
}
