using Growthstories.Sync;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace Growthstories.Sync
{
    public class HttpPullResponse : JsonResponse, ISyncPullResponse
    {
        private IList<IEventDTO> _Events = new List<IEventDTO>();
        protected JObject JResponse;

        public HttpPullResponse() { }
        public HttpPullResponse(HttpResponseMessage response, string body)
            : base(response, body)
        {

        }

        [JsonProperty(PropertyName = "cmds")]
        public IList<IEventDTO> Events
        {
            get
            {
                return _Events;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                _Events = value;
            }
        }

        protected override void Load()
        {

            _Events = new List<IEventDTO>();
            var serializer = new JsonSerializer()
                    {
                        DateParseHandling = DateParseHandling.DateTimeOffset,
                        DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat
                    };

            JResponse = JObject.Load(new JsonTextReader(new StringReader(Body))
            {
                DateParseHandling = DateParseHandling.DateTimeOffset,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            });
            if (JResponse["cmds"] == null)
            {
                throw new InvalidOperationException(Body);
            }

            EventDTO dto = null;
            JObject jdto = null;
            foreach (var token in (JArray)JResponse["cmds"])
            {
                jdto = (JObject)token;
                dto = EventDTO.Creators.Select(f => f(jdto)).First(x => x != null);
                serializer.Populate(new JTokenReader(jdto), dto);
                _Events.Add(dto);
            };

        }


    }
}
