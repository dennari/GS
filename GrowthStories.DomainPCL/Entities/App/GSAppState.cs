using CommonDomain;
using EventStore;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Entities
{




    public class GSAppState : AggregateState<GSAppCreated>
    {

        public static readonly Guid GSAppId = new Guid("10000000-0000-0000-0000-000000000001");

        //public readonly IDictionary<Guid, SyncStreamType> SyncStreams;

        public GSAppState()
            : base()
        {
        }

        public GSAppState(GSAppCreated e)
            : this()
        {
            this.Apply(e);
            //this.SyncStreams = new Dictionary<Guid, SyncStreamType>();

        }


        public override void Apply(GSAppCreated @event)
        {

            if (@event.EntityId != GSAppId)
                throw new ArgumentException(string.Format("There can only be a sing GSApp aggregate per installation and it has to be assigned id {0}", GSAppId));
            base.Apply(@event);
        }

        //public void Apply(SyncStreamCreated @event)
        //{
        //    SyncStreams[@event.AggregateId] = @event.StreamType;
        //}



    }
}
