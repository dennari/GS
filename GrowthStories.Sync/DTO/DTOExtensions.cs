using System;
using System.Collections.Generic;
using System.Reflection;
using Growthstories.Core;
using Growthstories.Domain.Messaging;

namespace Growthstories.Sync
{
    public static class DTOTypeExtensions
    {




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
