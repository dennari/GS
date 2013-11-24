using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Growthstories.Core;
using CommonDomain;

namespace Growthstories.Domain.Entities
{
    /// <summary>
    /// <para>The User data type contains information about a user.</para> <para> This data type is used as a response element in the following
    /// actions:</para>
    /// <ul>
    /// <li> <para> CreateUser </para> </li>
    /// <li> <para> GetUser </para> </li>
    /// <li> <para> ListUsers </para> </li>
    /// 
    /// </ul>
    /// </summary>
    public class User : AggregateBase<UserState, UserCreated>,
        ICommandHandler<CreateUser>,
        ICommandHandler<BecomeFollower>,
        ICommandHandler<SetUsername>,
        ICommandHandler<SetEmail>,
        ICommandHandler<SetPassword>,
        ICommandHandler<RequestCollaboration>,
        ICommandHandler<DenyCollaboration>,
        ICommandHandler<AddGarden>,
        ICommandHandler<CreateSchedule>,
        ICommandHandler<AddPlant>,
        ICommandHandler<CreateGarden>,
        ICommandHandler<MarkGardenPublic>
    {


        public User()
        {
            this.SyncStreamType = Core.PullStreamType.USER;
        }

        public void Handle(AddPlant command)
        {
            RaiseEvent(new PlantAdded(command));

        }

        public void Handle(CreateGarden command)
        {
            RaiseEvent(new GardenCreated(command));
        }

        public void Handle(MarkGardenPublic command)
        {
            RaiseEvent(new MarkedGardenPublic(command));
        }

        public void Handle(CreateSchedule command)
        {
            RaiseEvent(new ScheduleCreated(command));
        }

        public void Handle(CreateUser command)
        {

            RaiseEvent(new UserCreated(command));

        }

        public void Handle(SetUsername command)
        {

            RaiseEvent(new UsernameSet(command));

        }

        public void Handle(SetPassword command)
        {

            RaiseEvent(new PasswordSet(command));

        }
        public void Handle(SetEmail command)
        {

            RaiseEvent(new EmailSet(command));

        }

        public void Handle(BecomeFollower command)
        {
            RaiseEvent(new BecameFollower(command));
        }

        public void Handle(RequestCollaboration command)
        {
            RaiseEvent(new CollaborationRequested(command));
        }

        public void Handle(DenyCollaboration command)
        {
            RaiseEvent(new CollaborationDenied(command));
        }

        public void Handle(AddGarden command)
        {
            RaiseEvent(new GardenAdded(command));
        }
    }

}
