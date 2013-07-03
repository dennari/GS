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
    public class Synchronizer : AggregateBase<SynchronizerState, SynchronizerCreated>, ICommandHandler<CreateSynchronizer>
    {

        public void Handle(CreateSynchronizer command)
        {
            if (command.EntityId == default(Guid))
                throw new ArgumentNullException();

            RaiseEvent(new SynchronizerCreated()
            {
                EntityId = command.EntityId
            });
        }

        public async Task<IList<ISyncRequest>> Handle(Synchronize command, ISynchronizerService syncService)
        {
            if (command.EntityId == default(Guid))
                throw new ArgumentNullException();

            var r = await syncService.Synchronize();

            RaiseUserSynced(r);


            return r;
        }

        private void RaiseUserSynced(IList<ISyncRequest> r)
        {
            ISyncPushRequest pReq = (r.Count == 1 ? r[0] : r.Last()) as ISyncPushRequest;

            try
            {
                var UE = pReq.EventsFromStreams().First(y => y is UserCreated) as UserCreated;
                RaiseEvent(new UserSynchronized()
                    {
                        UserId = UE.EntityId,
                        Username = UE.Username,
                        Password = UE.Password,
                        Email = UE.Email,
                        EntityId = this.Id
                    });
            }
            catch (Exception) { }

        }




    }

}
