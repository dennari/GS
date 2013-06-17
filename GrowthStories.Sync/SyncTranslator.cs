using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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



        public IEnumerable<IEventDTO> Out(IEnumerable<IDomainEvent> events)
        {
            foreach (var e in events)
            {
                IEventDTO ed = null;
                try
                {
                    ed = e.ToDTO();
                    ed.AncestorId = Ancestor.Id;
                    var edd = ed as IAddEntityDTO;
                    if (edd != null)
                    {
                        edd.ParentAncestorId = Ancestor.Id;
                    }

                }
                catch (Exception) { }
                if (ed != null)
                {
                    yield return ed;
                }
            }
        }

        public IEnumerable<IDomainEvent> In(IEnumerable<IEventDTO> dtos)
        {

            foreach (var dto in dtos)
            {
                var union = dto as EventDTOUnion;
                if (union != null)
                {
                    yield return union.ToEvent();
                }

            }
        }






    }
}
