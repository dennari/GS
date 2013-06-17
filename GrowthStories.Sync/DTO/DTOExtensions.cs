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
                    var instance = (IDomainEvent)Activator.CreateInstance(T);
                    instance.FromDTO(dt);
                    return instance;
                }
                catch (Exception) { }
            }

            return null;

        }

        public static IEventDTO ToDTO(this IDomainEvent e)
        {
            try
            {
                var instance = new EventDTOUnion();
                instance.EventType = e.GetDTOType();
                e.FillDTO(instance);
                return instance;
            }
            catch (Exception) { }

            return null;
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

        public static DTOType GetDTOType(this IEvent e)
        {
            return e.GetAttribute<DTOObjectAttribute>().Type;
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
                        var DTOT = TI.GetCustomAttribute<DTOObjectAttribute>().Type;
                        IList<Type> TypeList = null;
                        if (DTOToEvent.TryGetValue(DTOT, out TypeList))
                        {
                            TypeList.Add(T);
                        }
                        else
                        {
                            DTOToEvent[DTOT] = new List<Type>() { T };
                        }
                    }
                    catch (Exception)
                    {
                        //Events.Add(T);
                    }

                }
            }

        }

        private static IDictionary<DTOType, IList<Type>> DTOToEvent = new Dictionary<DTOType, IList<Type>>();
    }

}
