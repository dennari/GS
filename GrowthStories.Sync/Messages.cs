using CommonDomain;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class CreateSynchronizer : EntityCommand
    {

        //protected CreateSynchronizer() { }
        //public CreateSynchronizer(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Create synchronizer {0}.", EntityId);
        }
    }

    public class SynchronizerCreated : EventBase
    {
        //protected SynchronizerCreated() { }
        //public SynchronizerCreated(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Created synchronizer {0}", EntityId);
        }
    }

    public class Synchronized : EventBase
    {
        //protected Synchronized() { }
        public Synchronized(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Synchronized at {0}", EntityId);
        }
    }

    //public delegate SyncPushRequest(){}

    public interface ISyncRequest
    {

    }

    public interface ISyncPushRequest : ISyncRequest
    {
        Task<ISyncPushResponse> Execute();
    }

    public interface ISyncPullRequest : ISyncRequest
    {
        Task<ISyncPullResponse> Execute();
    }

    public interface ISyncResponse
    {
    }

    public interface ISyncPushResponse : ISyncResponse
    { }

    public interface ISyncPullResponse : ISyncResponse
    { }


    public interface IEventDTO
    {
        /**
         * Kind of the object being modified
         * or created with this command
         * 
         * (part of entity's key)
         */
        string getKind();
        /**
         * Return the user who is the creator-owner of 
         * the object this command is operating on
         *
         * (part of entity's key)
         */
        long getTargetAncestorId();



        /**
         * Id of entity to be modified or created
         * is either a GUID (string) or a datastore id (int)
         *
         * (part of entity's key)
         */
        String getTargetEntityId();

        /**
         * Id of the object 
         * whose child the new object is going to be 
         */
        String getParentId();


        long getParentAncestorId();


    }


}
