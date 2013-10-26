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


        private static ILog Logger = LogFactory.BuildLogger(typeof(SyncTranslator));


        public IEventDTO Out(IEvent e)
        {
            var ee = (IDomainEvent)e;
            IEventDTO ed = ee.ToDTO();
            Logger.Info("Translated {0} to {1}", e.ToString(), ed == null ? "null" : ed.ToString());
            if (ed == null)
                return null;

            ed.AggregateVersion -= 1;


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


        public IEnumerable<IEventDTO> Out(IEnumerable<IStreamSegment> streams)
        {

            //IEventDTO ed = null;

            foreach (var stream in streams)
            {
                int i = 0;
                int zero = stream.AggregateVersion + stream.TranslateOffset - stream.Count;

                foreach (var x in stream)
                {
                    //try
                    //{
                    i++;

                    var e = x as IEvent;
                    if (e == null)
                        continue;
                    var ed = Out(e);
                    if (ed == null)
                        continue;
                    ed.AggregateVersion = zero + i;
                    // TRANSLATE PHASE RENUMBERING IS POSSIBLE



                    yield return ed;
                }
            }

        }

        public IEvent In(IEventDTO dto)
        {
            var e = ((EventDTOUnion)dto).ToEvent();
            e.AggregateVersion += 1;
            return e;

        }

        public IGrouping<Guid, IEvent>[] In(IEnumerable<IEventDTO> enumerable)
        {

            return enumerable
                .Select(x => In(x))
                .OfType<EventBase>()
                .GroupBy(x => x.StreamEntityId ?? x.AggregateId)
                .ToArray();


        }
    }
}
