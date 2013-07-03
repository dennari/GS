using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{



    #region User

    public class SynchronizerCreated : EventBase
    {

        public SynchronizerCreated() { }
        public SynchronizerCreated(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Created Synchronizer {0}", EntityId);
        }

    }

    public class Synchronized : EventBase
    {
        public Synchronized() { }
        public Synchronized(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Synchronized.", EntityId);
        }

    }

    public class UserSynchronized : EventBase
    {

        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }


        public UserSynchronized() { }
        public UserSynchronized(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Synchronized user {0}", UserId);
        }

    }



    #endregion

}

