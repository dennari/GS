using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Growthstories.Sync
{
    public class JsonRequest : HttpRequestMessage
    {



        public JsonRequest(ISyncRequest innerRequest)
        {
            // TODO: Complete member initialization
            InnerRequest = innerRequest;
            Method = HttpMethod.Post;
        }

        // Summary:
        //     Gets or sets the contents of the HTTP message.
        //
        // Returns:
        //     Returns System.Net.Http.HttpContent.The content of a message
        public new HttpContent Content
        {

            get
            {
                return new StringContent(toJSON(), Encoding.UTF8, "application/json");
            }

        }

        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        private ISyncRequest InnerRequest;

        public ISyncRequest Inner { get { return InnerRequest; } }

        public virtual string toJSON()
        {

            return JsonConvert.SerializeObject(InnerRequest, JsonSettings);
        }

    }
}
