using Growthstories.Sync;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace Growthstories.Sync
{
    public class SyncPushResponse : ISyncPushResponse
    {
        private readonly JObject Response;
        private readonly Guid RequestPushId;

        public SyncPushResponse(Guid requestPushId, JObject response)
        {
            Response = response;
            RequestPushId = requestPushId;
        }

            

        public bool IsValid()
        {
            try
            {
                return RequestPushId == PushId;

            }
            catch (Exception)
            {

                return false;
            }
        }



        public Guid ClientDatabaseId
        {

            get
            {
                try
                {
                    return Guid.Parse((string)Response["clientDatabaseId"]);
                }
                catch (Exception)
                {

                    return default(Guid);
                }

            }
        }

        public Guid PushId
        {
            get
            {
                try
                {
                    return Guid.Parse((string)Response["pushId"]);
                }
                catch (Exception)
                {

                    return default(Guid);
                }
            }
        }


        public IEnumerable<IEventDTO> Events
        {
            get
            {
                var serializer = new JsonSerializer() { DateParseHandling = DateParseHandling.DateTimeOffset, DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind, DateFormatHandling = DateFormatHandling.IsoDateFormat };
                EventDTO dto = null;
                JObject jdto = null;
                foreach (var token in (JArray)Response["cmds"])
                {
                    jdto = (JObject)token;
                    dto = EventDTO.Creators.Select(f => f(jdto)).First(x => x != null);
                    serializer.Populate(new JTokenReader(jdto), dto);
                    yield return dto;
                };
            }
        }
    }
}
