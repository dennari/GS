using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Growthstories.Sync
{
    public interface IEventDTO
    {
        IEvent ToEvent();
    }


    public class EventDTO : IEventDTO
    {


        protected IEvent _Event;
        protected JObject _JEvent;

        public EventDTO(JObject jEvent) : this(jEvent, null) { }

        public EventDTO(JObject jEvent, JsonSerializer serializer)
        {
            _JEvent = jEvent;
            _Serializer = serializer;
        }


        public EventDTO(IEvent @event)
        {
            _Event = @event;
        }


        public IEvent ToEvent()
        {
            if (_Event == null)
                _Event = ParseEvent();
            return _Event;
        }

        protected JsonSerializer _Serializer;
        protected JsonSerializer Serializer
        {
            get
            {
                return _Serializer == null ? _Serializer = JsonSerializer.CreateDefault() : _Serializer;
            }
        }

        protected IEvent ParseEvent()
        {
            foreach (var T in Language.PublicEvents)
            {
                try
                {
                    var @event = (IEvent)Serializer.Deserialize(new JTokenReader(_JEvent), T);
                    return @event;
                }
                catch (JsonSerializationException)
                {


                }
            }
            throw new JsonSerializationException("The provided JSON doesn't map to any known event");
        }



        public CommonDomain.IMemento Ancestor { get; set; }
    }
}