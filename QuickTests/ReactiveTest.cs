using System;
using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using System.Threading;
using NUnit.Framework;
using ReactiveUI;
using System.Collections.Generic;


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

        public static IObservable<T> DumpDo<T>(this IObservable<T> source, string name = "sequence")
        {

            return source.Do(
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
        public void TestSelectMany2()
        {

            int[][] a = new[] { new[] { 1, 2, 3 }, new[] { 1, 2, 3 } };

            var s = a.ToObservable().DumpDo("outer");

            var ss = Observable.Concat(s.Select(x =>
            {
                return x.ToObservable().DumpDo("inner");

            }));
            //var ss = s.Select(x => x.ToObservable()).Merge();



            using (var handle = ss.Dump("concat"))
            {
                //ss.Wait();
            }


        }

        [Test]
        public void TestEventListen()
        {
            var mb = new MessageBus();
            var observable1 = mb.Listen<Event>();
            //var observable2 = mb.Listen<PlantCreated>().DumpDo("original2");

            //var filtered = observable1.Where(x => x.Name == "jore").DumpDo("filtered");
            observable1
            .DumpDo("first")
            .Subscribe(x =>
            {
                "FIRST: {0}".Out(x.Name);
            });

            mb.SendMessage(new Event() { Name = "JORRE" });


            observable1
            .DumpDo("second")
           .Subscribe(x =>
           {
               "SECOND: {0}".Out(x.Name);
           });

            mb.SendMessage(new Event() { Name = "jore" });
            //mb.SendMessage(new Event() { Name = "jare" });


        }


    }

    public class Event
    {
        public string Name { get; set; }
    }


}
