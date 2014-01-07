using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Growthstories.Sync
{
    public class JsonRequest : HttpRequestMessage
    {
      ;


        public JsonRequest(ISyncRequest innerRequest,)
        {
            JsonSettings = jsonSettings;
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

            }

        }



    }
}
