using CommonDomain;
using EventStore;
using EventStore.Logging;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
//using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Growthstories.Sync
{
    public class SyncTranslator : ITranslateEvents
    {
        private readonly IUserService UserService;
        private readonly IConstructSyncEventStreams StreamFactory;

        private static ILog Logger = LogFactory.BuildLogger(typeof(SyncTranslator));


        public SyncTranslator(IUserService ancestorFactory, IConstructSyncEventStreams streamFactory)
        {
            this.UserService = ancestorFactory;
            this.StreamFactory = streamFactory;
        }



        public IEventDTO Out(IEvent e)
        {
            IEventDTO ed = ((IDomainEvent)e).ToDTO();
            Logger.Info("Translated {0} to {1}", e.ToString(), ed == null ? "null" : ed.ToString());
            if (ed == null)
                return null;

            ed.EntityVersion -= 1;
            ed.AncestorId = UserService.CurrentUser.Id;
            ed.StreamEntity = e.EntityId;

            var edd = ed as IAddEntityDTO;
            if (edd != null)
            {
                edd.ParentAncestorId = ed.AncestorId;
            }
            if (e is BecameFollower)
            {
                var eddd = ed as IAddEntityDTO;
                edd.ParentAncestorId = default(Guid);
                eddd.ParentId = UserService.CurrentUser.Id;
                eddd.EntityId = Guid.NewGuid();
            }

            return ed;

        }


        public IEnumerable<IEventDTO> Out(IEnumerable<IEvent> events)
        {
            IEventDTO ed = null;
            foreach (var e in events)
            {
                //try
                //{
                ed = Out(e);
                //}
                //catch (Exception) { }
                if (ed != null)
                {
                    yield return ed;
                }
            }
        }

        public IEnumerable<IEventDTO> Out(IEnumerable<ISyncEventStream> streams)
        {

            foreach (var stream in streams)
            {
                foreach (var e in Out(stream.CommittedEvents.Select(x => (IDomainEvent)x.Body)))
                {
                    //e.EntityVersion += stream.UncommittedRemoteEvents.Count;
                    yield return e;
                }
            }

        }

        public IEvent In(IEventDTO dto)
        {
            var e = ((EventDTOUnion)dto).ToEvent();
            e.EntityVersion += 1;
            return e;

        }

        public ICollection<ISyncEventStream> In(IEnumerable<IEventDTO> enumerable)
        {

            return enumerable
                .Select(x => In(x))
                .GroupBy(x => x.EntityId)
                .Select(x => this.StreamFactory.CreateStreamFromRemoteEvents(x))
                .ToArray();


        }
    }
}
