using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SyncTranslator : ITranslateEvents
    {


        [Inject]
        [Named("CurrentUser")]
        public IMemento Ancestor { get; set; }


        public IEventDTO Out(IEvent msg)
        {
            return ((dynamic)this).TranslateOut((dynamic)msg);
        }


        protected IEventDTO TranslateOut(PlantAdded @event)
        {
            return TranslateOutHelper(new PlantAddedDTO(@event));
        }

        protected IEventDTO TranslateOut(MarkedPlantPublic @event)
        {
            return TranslateOutHelper(new SetPropertyDTO(@event));
        }

        private IEventDTO TranslateOutHelper(IEventDTO p)
        {
            p.targetAncestorId = Ancestor.Id;
            p.parentAncestorId = Ancestor.Id;
            return p;
        }

        public ICollection<IEventDTO> Out(IEnumerable<IEvent> msgs)
        {
            var dtos = new List<IEventDTO>();
            foreach (var @event in msgs)
            {
                try
                {
                    dtos.Add(Out(@event));
                }
                catch (Exception)
                {

                    continue;
                }

            }
            return dtos;
        }


        public IEvent In(IEventDTO msg)
        {
            return ((dynamic)this).TranslateIn((dynamic)msg);
        }


        public ICollection<IEvent> In(IEnumerable<IEventDTO> msgs)
        {
            var events = new List<IEvent>();
            foreach (var @event in msgs)
            {
                try
                {
                    events.Add(In(@event));
                }
                catch (Exception)
                {

                    continue;
                }

            }
            return events;
        }






    }
}
