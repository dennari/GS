using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using Growthstories.Sync;
using Newtonsoft.Json;


namespace Growthstories.Domain.Messaging
{



    #region User

    public class SynchronizerCreated : EventBase
    {

        protected SynchronizerCreated() { }
        public SynchronizerCreated(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Created Synchronizer {0}", AggregateId);
        }

    }

    public class Synchronized : EventBase
    {
        protected Synchronized() { }
        public Synchronized(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Synchronized.", AggregateId);
        }

    }

    public class UserSynchronized : EventBase
    {

        [JsonProperty]
        public Guid UserId { get; private set; }
        [JsonProperty]
        public string Username { get; private set; }
        [JsonProperty]
        public string Password { get; private set; }
        [JsonProperty]
        public string Email { get; private set; }


        protected UserSynchronized() { }
        public UserSynchronized(Guid entityId, Guid userId, string username, string password, string email) :
            base(entityId)
        {
            this.UserId = userId;
            this.Username = username;
            this.Password = password;
            this.Email = email;
        }

        public override string ToString()
        {
            return string.Format(@"Synchronized user {0}", UserId);
        }

    }



    #endregion

}

