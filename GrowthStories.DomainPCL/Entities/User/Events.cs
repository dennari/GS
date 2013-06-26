using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;


namespace Growthstories.Domain.Messaging
{



    #region User


    public class UserCreated : EventBase
    {

        protected UserCreated() { }
        public UserCreated(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Created user {0}", EntityId);
        }

    }

    #endregion

}

