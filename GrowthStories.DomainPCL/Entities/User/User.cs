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
        ICommandHandler<BecomeFollower>
    {

        public void Handle(CreateUser command)
        {

            RaiseEvent(new UserCreated()
            {
                EntityId = command.EntityId,
                Username = command.Username,
                Password = command.Password,
                Email = command.Email
            });
        }

        public void Handle(SetAuthToken command)
        {
            RaiseEvent(new AuthTokenSet(command));
        }

        public void Handle(BecomeFollower command)
        {
            RaiseEvent(new BecameFollower(command));
        }
    }

}
