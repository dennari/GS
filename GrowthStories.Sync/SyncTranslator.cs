using CommonDomain;
using EventStore;
using EventStore.Logging;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
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
            var ee = (IDomainEvent)e;
            IEventDTO ed = ee.ToDTO();
            Logger.Info("Translated {0} to {1}", e.ToString(), ed == null ? "null" : ed.ToString());
            if (ed == null)
                return null;

            ed.EntityVersion -= 1;
            ed.StreamEntity = e.EntityId;
            ed.AncestorId = UserService.CurrentUser.Id;
            if (ee.HasAncestor)
            {

                ed.StreamAncestor = ed.AncestorId;
            }

            if (ee.HasParent && ed is IAddEntityDTO)
            {
                ((IAddEntityDTO)ed).ParentAncestorId = ed.AncestorId;
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
