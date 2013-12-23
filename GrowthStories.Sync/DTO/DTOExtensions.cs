using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Growthstories.Sync
{
    public static class DTOTypeExtensions
    {

        public static IDomainEvent ToEvent(this EventDTOUnion dt)
        {
            foreach (var T in EventCache.GetEvents(dt.EventType))
            {
                try
                {
                    var instance = (IDomainEvent)JsonConvert.DeserializeObject("{}", T, new JsonSerializerSettings() { ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor });
                    instance.FromDTO(dt);
                    return instance;
                }
                catch (Exception) { }
            }

            return null;

        }

        public static IEventDTO ToDTO(this IDomainEvent e)
        {
            //try
            //{
            DTOType[] T = e.GetDTOType();
            if (T.Length == 0)
                return null;
            var instance = new EventDTOUnion()
            {
                EventType = T[0]
            };
            e.FillDTO(instance);
            return instance;
            //}
            //catch (Exception) { }

            //return null;
        }


        public static T GetAttribute<T>(this Type o) where T : Attribute
        {
            return o
                .GetTypeInfo()
                .GetCustomAttribute<T>();

        }

        public static T GetAttribute<T>(this object o) where T : Attribute
        {
            return o
                .GetType()
                .GetTypeInfo()
                .GetCustomAttribute<T>();
        }

        public static DTOType[] GetDTOType(this IEvent e)
        {

            DTOObjectAttribute attr = e.GetAttribute<DTOObjectAttribute>();

            return attr != null ? attr.Type : new DTOType[0];
        }

    }

    public class EventCache
    {

        public static IList<Type> GetEvents(DTOType T)
        {
            if (DTOToEvent.Count == 0)
            {
                FillEvents();
            }
            return DTOToEvent[T];
        }

        private static void FillEvents()
        {
            var IETI = typeof(IDomainEvent).GetTypeInfo();

            foreach (var T in typeof(EventBase).GetTypeInfo().Assembly.ExportedTypes)
            {
                var TI = T.GetTypeInfo();
                if (IETI.IsAssignableFrom(TI))
                {
                    try
                    {
                        if (TI != null && TI.GetCustomAttribute<DTOObjectAttribute>() != null)
                        {
                            var DTOTs = TI.GetCustomAttribute<DTOObjectAttribute>().Type;
                            IList<Type> TypeList = null;
                            foreach (var DTOT in DTOTs)
                            {
                                if (DTOToEvent.TryGetValue(DTOT, out TypeList))
                                {
                                    TypeList.Add(T);
                                }
                                else
                                {
                                    DTOToEvent[DTOT] = new List<Type>() { T };
                                }
                            }
                        }                       
                    }

                    catch (Exception)
                    {
                        // TODO: is this appropriate?
                        //       should we log?
                    }

                }
            }

        }

        private static IDictionary<DTOType, IList<Type>> DTOToEvent = new Dictionary<DTOType, IList<Type>>();
    }

}
