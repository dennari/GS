using Growthstories.Domain.Messaging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Growthstories.Core;
using CommonDomain;
using Growthstories.Sync;
using System.Threading.Tasks;

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
    public class Synchronizer : AggregateBase<SynchronizerState, SynchronizerCreated>,
        ICommandHandler<CreateSynchronizer>,
        ICommandHandler<Synchronize>
    {

        public void Handle(CreateSynchronizer command)
        {
            if (command.EntityId == default(Guid))
                throw new ArgumentNullException();

            RaiseEvent(new SynchronizerCreated(command.EntityId));

        }


        public void Handle(Synchronize command)
        {
            throw new NotImplementedException();
        }
    }

}
