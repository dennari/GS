using CommonDomain;
using EventStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static string ToStringExtended(this Exception This)
        {
            StringBuilder sb = new StringBuilder();
            CreateExceptionString(sb, This);

            return sb.ToString();
        }


        /// <summary>
        ///  From: http://stackoverflow.com/questions/2989401/how-to-log-exception-in-a-file
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="e"></param>
        /// <param name="indent"></param>
        private static void CreateExceptionString(StringBuilder sb, Exception e, string indent = null)
        {
            if (indent == null)
            {
                indent = string.Empty;
            }
            else if (indent.Length > 0)
            {
                sb.AppendFormat("{0}Inner ", indent);
            }

            sb.AppendFormat("Exception Found:\n{0}Type: {1}", indent, e.GetType().FullName);
            sb.AppendFormat("\n{0}Message: {1}", indent, e.Message);
            sb.AppendFormat("\n{0}Source: {1}", indent, e.Source);
            sb.AppendFormat("\n{0}Stacktrace: {1}", indent, e.StackTrace);

            if (e.InnerException != null)
            {
                sb.Append("\n");
                CreateExceptionString(sb, e.InnerException, indent + "  ");
            }
        }
    }
}
