using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;

namespace Growthstories.Sync
{
    public class DTOConverter : CustomCreationConverter<EventDTOUnion>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IEventDTO);
        }

        public override EventDTOUnion Create(Type objectType)
        {
            if (objectType == typeof(IEventDTO))
            {
                return new EventDTOUnion();
            }
            return null;
        }
    }
}
