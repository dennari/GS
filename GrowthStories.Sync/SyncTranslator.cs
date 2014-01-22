using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EventStore.Logging;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using ReactiveUI;


namespace Growthstories.Sync
{
    public class SyncTranslator : ITranslateEvents
    {
        readonly MemoizingMRUCache<Type, Func<object>> ConstructorCache;

        public SyncTranslator()
        {
            this.ConstructorCache = new MemoizingMRUCache<Type, Func<object>>((t, _) => CompileConstructor(t), RxApp.BigCacheLimit);
        }

        private Func<object> CompileConstructor(Type type)
        {

            var defaultCtor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Length == 0);
            if (defaultCtor == null)
                return null;
            return (Func<object>)Expression.Lambda(typeof(Func<object>), Expression.New(defaultCtor)).Compile();
        }

        private IDomainEvent ToEvent(EventDTOUnion dt)
        {
            if (dt == null)
                return null;

            foreach (var T in EventCache.GetEvents(dt.EventType))
            {

                //var instance = (IDomainEvent)JsonConvert.DeserializeObject("{}", T, new JsonSerializerSettings() { ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor });
                var ctor = ConstructorCache.Get(T);
                if (ctor == null)
                    continue;
                var instance = ctor() as IDomainEvent;
                if (instance == null)
                    continue;
                var success = instance.FromDTO(dt);
                if (!success)
                    continue;
                return instance;

            }


            return null;

        }

        private IEventDTO ToDTO(IDomainEvent e)
        {
            if (e == null)
                return null;
            DTOType[] T = e.GetDTOType();
            if (T.Length == 0)
                return null;
            var instance = new EventDTOUnion()
            {
                EventType = T[0]
            };
            var success = e.FillDTO(instance);
            if (success)
                return instance;


            return null;
        }

        private static ILog Logger = LogFactory.BuildLogger(typeof(SyncTranslator));


        public IEventDTO Out(IEvent e)
        {
            IEventDTO ed = ToDTO(e as IDomainEvent);
            Logger.Info("OUT-Translated {0}", e.ToString());
            if (ed == null)
                return null;

            ed.AggregateVersion -= 1;

            return ed;
        }


        public IEnumerable<IEventDTO> Out(IEnumerable<IEvent> events)
        {
            IEventDTO ed = null;
            foreach (var e in events)
            {
                //try
                //{
                ed = Out(e);
                //}
                //catch (Exception) { }
                if (ed != null)
                {
                    yield return ed;
                }
            }
        }


        public IEnumerable<IEventDTO> Out(IEnumerable<IStreamSegment> streams)
        {

            foreach (var stream in streams)
            {
                int i = 0;
                int zero = stream.AggregateVersion + stream.TranslateOffset - stream.Count;

                foreach (var x in stream)
                {
                    //try
                    //{
                    i++;

                    var e = x as IEvent;
                    if (e == null)
                        continue;
                    var ed = Out(e);
                    if (ed == null)
                        continue;
                    ed.AggregateVersion = zero + i - 1; // minus one comes from the fact that the backend starts counting from zero
                    // TRANSLATE PHASE RENUMBERING IS POSSIBLE



                    yield return ed;
                }
            }

        }

        public IEvent In(IEventDTO dto)
        {
            var e = ToEvent(dto as EventDTOUnion);
            Logger.Info("IN-Translated {0}", e.ToString());
            e.AggregateVersion += 1;
            return e;

        }


        public IGrouping<Guid, IEvent>[] In(IEnumerable<IEventDTO> enumerable)
        {
            return enumerable
                .Select(x => In(x))
                .OfType<EventBase>()
                .GroupBy(x => x.StreamEntityId ?? x.AggregateId)
                .ToArray();
        }
    }
}
