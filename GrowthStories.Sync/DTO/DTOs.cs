using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Growthstories.Sync
{
    public class PlantAddedDTO : EventDTO
    {
        public PlantAddedDTO() { }
        public PlantAddedDTO(PlantAdded @event)
            : base(@event)
        {
            type = "addPlant";
            name = @event.PlantName;
            parentId = @event.EntityId;
        }

        public new static bool Matches(JObject o)
        {
            return EventDTO.Matches(o) && (string)o["type"] == "addPlant" && o["name"] != null;
        }

        public static EventDTO CreateIfMatches(JObject o)
        {
            if (Matches(o))
                return new PlantAddedDTO();
            return null;
        }

        public string name { get; set; }
    }

    public class SetPropertyDTO : EventDTO
    {
        public const string TYPE = "SetProperty";

        public SetPropertyDTO()
        {
            type = TYPE;
        }
        public SetPropertyDTO(IEvent @event)
            : base(@event)
        {
            type = TYPE;
        }

        public SetPropertyDTO(MarkedPlantPublic @event)
            : this((IEvent)@event)
        {
            kind = "plant";
            propName = "shared";
            propValue = "true";
        }

        public SetPropertyDTO(MarkedPlantPrivate @event)
            : this((IEvent)@event)
        {
            kind = "plant";
            propName = "shared";
            propValue = "false";
        }

        public new static readonly string[] Required = new string[] { "propName", "propValue", "kind" };


        public new static bool Matches(JObject o)
        {
            o.
            return EventDTO.Matches(o) && (string)o["type"] == "SetProperty" && Required.All(x => o[x] != null);
        }

        public string kind { get; set; }
        public string propName { get; set; }
        public string propValue { get; set; }

        public static EventDTO CreateIfMatches(JObject o)
        {
            if (Matches(o))
                return new SetPropertyDTO();
            return null;
        }
    }


    public class CommentAddedDTO : EventDTO
    {
        private static Dictionary<string, string> _required = new Dictionary<string, string> {
            {"type","addComment"} 
        };

        public CommentAddedDTO() { }

        public CommentAddedDTO(IEvent @event)
            : base(@event)
        {
            var type = GetType().GetTypeInfo();
            foreach (var x in _required)
            {
                type.GetDeclaredProperty(x.Key).SetValue(this, x.Value);
            }
        }

        public new static bool Matches(JObject o)
        {
            return EventDTO.Matches(o) && _required.All(x => (string)o[x.Key] == x.Value) && o["note"] != null;
        }

        public static EventDTO CreateIfMatches(JObject o)
        {
            if (Matches(o))
                return new CommentAddedDTO();
            return null;
        }

        public string note { get; set; }
    }
}