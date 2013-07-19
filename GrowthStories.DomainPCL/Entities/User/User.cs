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
        ICommandHandler<SetAuthToken>,
        ICommandHandler<BecomeFollower>,
        ICommandHandler<AddGarden>,
        ICommandHandler<Comment>,
        ICommandHandler<Water>,
        ICommandHandler<Photograph>,
        ICommandHandler<Fertilize>
    {

        public void Handle(CreateUser command)
        {

            RaiseEvent(new UserCreated(command));

        }

        public void Handle(SetAuthToken command)
        {
            RaiseEvent(new AuthTokenSet(command));
        }

        public void Handle(BecomeFollower command)
        {
            RaiseEvent(new BecameFollower(command));
        }

        public void Handle(Comment command)
        {
            RaiseEvent(new Commented(command));
        }

        public void Handle(Water command)
        {
            RaiseEvent(new Watered(command));
        }

        public void Handle(Photograph command)
        {
            RaiseEvent(new Photographed(command));
        }

        public void Handle(Fertilize command)
        {
            RaiseEvent(new Fertilized(command));
        }


        public void Handle(AddGarden command)
        {
            RaiseEvent(new GardenAdded(command));
        }
    }

}
