using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;



namespace Growthstories.Sync
{
    public interface ISyncRequest
    {

    }

    public interface ISyncPushRequest : ISyncRequest
    {

        Guid clientDatabaseId { get; }
        Guid pushId { get; }
        ICollection<IEventDTO> cmds { get; }

        ISyncPushResponse Execute();
    }

    public interface ISyncPullRequest : ISyncRequest
    {
        Task<ISyncPullResponse> Execute();
    }


    public class SyncPushRequest : ISyncPushRequest
    {

        public Guid clientDatabaseId { get; private set; }
        public Guid pushId { get; private set; }

        private readonly ICollection<IEventDTO> _cmds;

        public SyncPushRequest()
            : this(new List<IEventDTO>())
        {

        }
        public SyncPushRequest(ICollection<IEventDTO> syncDTOs)
        {
            // TODO: Complete member initialization
            _cmds = syncDTOs;
            pushId = Guid.NewGuid();
            //clientDatabaseId = Guid.NewGuid();

        }
        public ICollection<IEventDTO> cmds { get { return _cmds; } }

        public ISyncPushResponse Execute()
        {

            HttpClient httpClient = new HttpClient();
            //var task = httpClient.


            HttpMessageHandler handler = new HttpClientHandler();
            //handler = new PlugInHandler(handler); // Adds a custom header to every request and response message.             
            httpClient = new HttpClient(handler);

            // The following line sets a "User-Agent" request header as a default header on the HttpClient instance. 
            // Default headers will be sent with every request sent from this HttpClient instance. 
            //httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Sample", "v8")); 

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None, DateFormatHandling = DateFormatHandling.IsoDateFormat, DateTimeZoneHandling = DateTimeZoneHandling.Utc };
            var str = JsonConvert.SerializeObject(this, settings);

            JsonReader reader = new JsonTextReader(new StringReader(str))
            {
                DateParseHandling = DateParseHandling.DateTimeOffset,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };


            return new SyncPushResponse(pushId, JObject.Load(reader));
        }

        public Task<ISyncPushResponse> ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }


}