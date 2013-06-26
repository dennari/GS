using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{


    #region User
    public class CreateUser : EntityCommand<User>
    {


        protected CreateUser() { }
        public CreateUser(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Create user {0}.", EntityId);
        }

    }


    public class DeleteUser : EntityCommand<User>
    {
        protected DeleteUser() { }
        public DeleteUser(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete user {0}.", EntityId);
        }

    }
    #endregion

}

