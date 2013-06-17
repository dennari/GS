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
using Newtonsoft.Json.Serialization;
using Growthstories.Domain.Messaging;


namespace Growthstories.Sync
{
    public abstract class JsonResponse
    {
        [JsonIgnore]
        public HttpResponseMessage HttpResponse { get; private set; }
        [JsonIgnore]
        public string Body { get; private set; }
        private readonly JsonSerializerSettings JsonSettings;
        protected JObject JResponse;

        public JsonResponse() { }
        public JsonResponse(HttpResponseMessage response, String body, JsonSerializerSettings jsonSettings)
        {
            if (response == null || body == null)
            {
                throw new ArgumentNullException("response can't be null");
            }
            HttpResponse = response;
            Body = body;
            Load();
            //this.parseResponse(response);
        }

        protected virtual void Load()
        {

            var JsonSettings = new JsonSerializerSettings();
            using (var sr = new StringReader(Body))
            using (var jr = new JsonTextReader(sr)
            {
                DateParseHandling = JsonSettings.DateParseHandling,
                DateTimeZoneHandling = JsonSettings.DateTimeZoneHandling,
                FloatParseHandling = JsonSettings.FloatParseHandling
            })
            {
                JResponse = JObject.Load(jr);
            }

            JToken events = null;
            if (!JResponse.TryGetValue(Language.EVENTS, out events))
                throw new JsonSerializationException("malformed response json");
            //_JEvents = (JArray)events;

            //JsonConvert.PopulateObject(Body, this, new JsonSerializerSettings
            //{
            //    ObjectCreationHandling = ObjectCreationHandling.Replace,
            //    ContractResolver = new CamelCasePropertyNamesContractResolver()
            //});
        }


    }
}
