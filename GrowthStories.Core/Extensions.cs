using CommonDomain;
using EventStore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Growthstories.Core
{
    public static class Extensions
    {

        public enum EventTypes
        {
            Committed,
            UnCommitted,
            All
        }

        public static IEnumerable<IEvent> Events(this IEventStream stream, EventTypes type = EventTypes.Committed)
        {

            IEnumerable<EventMessage> sequence = stream.CommittedEvents;
            if (type == EventTypes.UnCommitted)
                sequence = stream.UncommittedEvents;
            else if (type == EventTypes.All)
                sequence = sequence.Concat(stream.UncommittedEvents);
            foreach (var e in sequence)
                yield return (IEvent)e.Body;
        }

        public static IEnumerable<IEvent> ActualEvents(this Commit commit)
        {

            foreach (var e in commit.Events)
                yield return (IEvent)e.Body;
        }

        public const double MillisecondsInWeek = 7 * 24 * 3600 * 1000;
        public const double MillisecondsInDay = 24 * 3600 * 1000;

        public static int TotalWeeks(this TimeSpan t)
        {
            return (int)(t.TotalMilliseconds / MillisecondsInWeek);
        }

        public static int DaysAfterWeeks(this TimeSpan t)
        {
            var ms = t.TotalMilliseconds;
            var w = t.TotalWeeks();
            return (int)((ms - w * MillisecondsInWeek) / MillisecondsInDay);
        }
    }
}
