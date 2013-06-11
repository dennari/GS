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


namespace Growthstories.Sync
{
    public abstract class JsonResponse
    {
        [JsonIgnore]
        public HttpResponseMessage HttpResponse { get; private set; }
        [JsonIgnore]
        public string Body { get; private set; }


        public JsonResponse() { }
        public JsonResponse(HttpResponseMessage response, String body)
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
            JsonConvert.PopulateObject(Body, this, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }


    }
}
