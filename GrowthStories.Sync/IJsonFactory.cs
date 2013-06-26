using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface IJsonFactory
    {
        string Serialize(object o);

        T Deserialize<T>(string i);
    }

    public class JsonFactory : IJsonFactory
    {
        private JsonSerializerSettings settings;

        protected static JsonSerializerSettings DefaultSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.None,
            Converters = new List<JsonConverter>() {
                    new StringEnumConverter()
                }
        };

        public JsonFactory()
            : this(DefaultSettings)
        {
        }


        public JsonFactory(JsonSerializerSettings settings)
        {
            this.settings = settings;
        }


        public string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o, settings);
        }

        public T Deserialize<T>(string i)
        {
            return JsonConvert.DeserializeObject<T>(i, settings);
        }
    }

}
