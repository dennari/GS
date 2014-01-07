using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{

    public class JsonFactory : IJsonFactory
    {
        private JsonSerializerSettings settings;

        protected static JsonSerializerSettings DefaultSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.None,
            DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
            Converters = new List<JsonConverter>() {
                    new StringEnumConverter(),
                    new IsoDateTimeConverter()
                    {
                        Culture = CultureInfo.InvariantCulture,
                        DateTimeFormat = "yyyyMMddHHmmssffff"
                    }
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
