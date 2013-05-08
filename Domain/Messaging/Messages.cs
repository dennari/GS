using Growthstories.Domain.Entities;
using Growthstories.Domain.Interfaces;
using System;


namespace Growthstories.Domain.Messaging
{

    public class CreateUser : ICommand<UserId>
    {
        public UserId EntityId { get; private set; }

        CreateUser() { }
        public CreateUser(UserId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Create user {0}.", EntityId);
        }
    }

    public partial class UserCreated : IEvent<UserId>
    {
        public UserId EntityId { get; private set; }

        UserCreated() { }
        public UserCreated(UserId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Created user {0}", EntityId);
        }
    }

}

