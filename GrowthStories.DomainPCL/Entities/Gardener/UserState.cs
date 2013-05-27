using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;


namespace Growthstories.Domain.Entities
{
    public class UserState : AggregateState<UserCreated>
    {

        public UserState() { }
        public UserState(Guid id, int version, bool Public) : base(id, version, Public) { }


    }
}
