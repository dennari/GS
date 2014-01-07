using System;
using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using System.Threading;
using NUnit.Framework;
using ReactiveUI;
using EventStore.Dispatcher;
using Growthstories.Core;
using EventStore;
using System.Collections.Generic;
using Growthstories.Domain.Messaging;

namespace QuickTests
{
    public static class ObservableExtensions
    {

        public static IDisposable Dump<T>(this IObservable<T> source, string name = "sequence")
        {

            return source.Subscribe(
                i => Out("{0}-->{1} @ {2}", name, i, NowTime()),
                ex => Out("{0} failed-->{1} @ {2}", name, ex.Message, NowTime()),
                () => Out("{0} completed @ {1}", name, NowTime()));

        }

        public static void Out(this string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public static string NowTime()
        {
            return DateTimeOffset.Now.ToString("H:mm:ss.fff");
        }
    }


    //[TestClass]
    public class ReactiveTest
    {
        [Test]
        public void TestTime()
        {

            var s = Observable
                 .Interval(TimeSpan.FromMilliseconds(500))
                 .Take(3);
            using (var handle = s.Dump())
            {
                s.Wait();
            }



        }

        [Test]
        public void TestCreate()
        {

            var s = Observable.Create<string>(obv =>
            {
                obv.OnNext("first");
                obv.OnNext("second");
                obv.OnCompleted();
                return () => "Disposed".Out();
            });

            using (var handle = s.Dump())
            {
                s.Wait();
            }


        }

        [Test]
        public void TestRange()
        {

            int from = 0;
            int to = 5;
            var s = Observable.Generate<int, int>(from, i => i < to, i => i + 1, i => i);


            using (var handle = s.Dump("range"))
            {
                s.Wait();
            }


        }


        [Test]
        public void TestSelectMany()
        {

            ("Test running on thread " + Thread.CurrentThread.ManagedThreadId.ToString()).Out();
            var s = Observable.Interval(TimeSpan.FromSeconds(3))
            .Select(i => i + 1) //Values start at 0, so add 1.
            .Take(3)            //We only want 3 values
            .SelectMany(
                offset => Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                            .Select(x => (offset * 10) + x)
                            .Take(3)
            );

            s.Subscribe(i => ("On thread " + Thread.CurrentThread.ManagedThreadId.ToString()).Out());

            using (var handle = s.Dump("range"))
            {
                s.Wait();
            }


        }




        [Test]
        public void TestEventDispatch()
        {

            // arrange
            IEvent[] events = new IEvent[] {
                new PlantCreated(Guid.NewGuid(),"jore",Guid.NewGuid()),
                new PlantCreated(Guid.NewGuid(),"jare",Guid.NewGuid())
            };
            var mb = new MessageBus();
            var observable = mb.Listen<PlantCreated>();

            var dispatcher = new MBDispatcher(mb);
            var commit = new Commit(
                Guid.Empty,
                2,
                Guid.Empty,
                1,
                DateTime.MaxValue,
                null,
                events.Select(x => new EventMessage() { Body = x }).ToList()
            );



            //observable.Subscribe(e => {
            //    Console.WriteLine(e);          
            //});
            bool asserted = false;
            observable.Dump();
            observable.Take(events.Length).ToArray().Subscribe(
                events2 =>
                {
                    Assert.True(events.Except(events2).Count() == 0);
                    Assert.True(events2.Except(events).Count() == 0);
                    asserted = true;
                },
                () =>
                {
                    Assert.True(asserted);
                    "Asserted".Out();
                }
            );

            "Before dispatch".Out();
            dispatcher.Dispatch(commit);
            "After dispatch".Out();

            "Before dispatch".Out();
            dispatcher.Dispatch(commit);
            "After dispatch".Out();

            Thread.Sleep(1000);
            Assert.True(asserted);

        }


    }

    public class MBDispatcher : IDispatchCommits
    {
        private readonly IMessageBus Mb;

        public MBDispatcher(IMessageBus mb)
        {
            this.Mb = mb;
        }

        public void Dispatch(Commit commit)
        {
            foreach (var e in commit.ActualEvents())
                this.Mb.SendMessage(((dynamic)e));
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

}
