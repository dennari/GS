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
using Ninject;


namespace Growthstories.Sync
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class HttpPullRequest : ISyncPullRequest
    {


        private readonly ITransportEvents Transporter;



        public HttpPullRequest(ITransportEvents transporter)
        {

            Transporter = transporter;

        }



        public Task<ISyncPullResponse> ExecuteAsync()
        {

            return Transporter.PullAsync(this);

        }
    }
}
