using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.DomainTests
{
    public class TestUtils
    {
        public static string randomize(string i)
        {
            //var b = new StringBuilder(i);
            //b.Append(Guid.NewGuid().ToString().Substring(0, 4));
            return i + Guid.NewGuid().ToString().Substring(0, 4);
        }

        public static UserCreated CreateUserFromName(string name)
        {
            return new UserCreated(new CreateUser(
                Guid.NewGuid(),
                name,
                randomize("swordfish"),
                randomize(name) + "@wonderland.net"))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };
        }

        public static T WaitForTask<T>(Task<T> task, int timeout = 9000)
        {
            if (Debugger.IsAttached)
                task.Wait();
            else
                task.Wait(timeout);
            //Assert.IsTrue(task.IsCompleted, "Task timeout");
            return task.Result;
        }

        public static void WaitForTask(Task task, int timeout = 9000)
        {
            if (Debugger.IsAttached)
                task.Wait();
            else
                task.Wait(timeout);
            //Assert.IsTrue(task.IsCompleted, "Task timeout");
        }

        public static T WaitForFirst<T>(IObservable<T> task, int timeout = 9000)
        {

            //task.Take(1).
            return WaitForTask(Task.Run(async () =>
            {

                return await task.Take(1);

            }), timeout);

        }

        public static IStreamSegment[] EventsToStreams(Guid aggregateId, IEvent events)
        {
            var msgs = new StreamSegment(aggregateId);
            msgs.Add(events);
            return new[] { msgs };
        }

        public static IStreamSegment[] EventsToStreams(Guid aggregateId, IEnumerable<IEvent> events)
        {
            var msgs = new StreamSegment(aggregateId);
            msgs.AddRange(events);
            return new[] { msgs };
        }

        public static IEnumerable<IStreamSegment> EventsToStreams(IEnumerable<IEvent> events)
        {

            foreach (var g in events.GroupBy(x => x.AggregateId))
            {
                var msgs = new StreamSegment(g.Key);
                msgs.AddRange(g);
                yield return msgs;
            }

        }
    }
}
