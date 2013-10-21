using System;
using System.Linq;
using NUnit.Framework;
using Ninject;
using ReactiveUI;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System.Reactive.Concurrency;
using Growthstories.Domain;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using Growthstories.Domain.Entities;
using CommonDomain;
using EventStore.Persistence;
using EventStore;

namespace Growthstories.DomainTests.Sync
{
    public class UnitTest1
    {
        [Test]
        public void TestRepositoryConcurrency()
        {
            var Kernel = new StandardKernel(new StagingModule());
            var cmdHandler = Kernel.Get<IDispatchCommands>();
            var bus = Kernel.Get<IMessageBus>();
            var repository = Kernel.Get<IGSRepository>();


            var scheduler = TaskPoolScheduler.Default;
            //var sc = TaskScheduler.Current;
            //var scheduler = CurrentThreadScheduler.Instance;
            //var scheduler = NewThreadScheduler.Default;

            bus.RegisterScheduler<IAggregateCommand>(scheduler);

            var handled = 0;
            var num = 20;

            var subj = new Subject<IAggregateCommand>();

            var tasks = new List<Task>();
            var rnd = new Random();

            var subscription = bus.Listen<IAggregateCommand>().Subscribe(x =>
            {

                //var xx = x as CreatePlant;
                //Console.WriteLine(string.Format("{0} -- Listening", xx.Name));

                //var task = Task.Run(() =>
                //{
                //var xx = x as CreatePlant;

                Console.WriteLine(string.Format("Handling cmd {0} on thread {1}", x, Thread.CurrentThread.ManagedThreadId));
                //Thread.Sleep(100 + rnd.Next(700));
                cmdHandler.Handle(x);

                Console.WriteLine(string.Format("Handled cmd {0} on thread {1}", x, Thread.CurrentThread.ManagedThreadId));

                Interlocked.Increment(ref handled);
                //});
                //task.ConfigureAwait(false);
                //tasks.Add(task);


            });



            var ids = Enumerable.Range(0, num)
                .Select(x => new { sequence = x, id = Guid.NewGuid(), name = "Jare " + x.ToString() }).ToArray();

            Console.WriteLine(string.Format("Running test on thread {0}", Thread.CurrentThread.ManagedThreadId));

            foreach (var id in ids)
            {

                bus.SendCommand((IAggregateCommand)new CreatePlant(id.id, id.sequence.ToString(), Guid.NewGuid(), Guid.NewGuid()));
                bus.SendCommand((IAggregateCommand)new SetName(id.id, id.name));


            }


            var numCmds = 2 * num;
            var maxSeconds = 60;
            var started = DateTime.Now;
            while (handled < numCmds && (DateTime.Now - started).Seconds < maxSeconds)
                Thread.Sleep(400);

            //for (var x = 0; x < num; x++)
            //{
            //    var xx = x;
            //    var task = Task.Run(() =>
            //    {
            //        //var xx = x as CreatePlant;

            //        Console.WriteLine(string.Format("{0} -- Handling on thread {1}", xx, Thread.CurrentThread.ManagedThreadId));
            //        //cmdHandler.Handle(x);
            //        Thread.Sleep(5000);
            //        Console.WriteLine(string.Format("{0} -- Handled on thread {1}", xx, Thread.CurrentThread.ManagedThreadId));

            //        //Interlocked.Increment(ref handled);
            //    });
            //    task.ConfigureAwait(false);
            //    tasks.Add(task);
            //}
            //if (tasks.Count == 0)
            //{
            //    Assert.Fail("No tasks to await");
            //}
            //Task.WaitAll(tasks.ToArray());

            //tasks.Clear();
            foreach (var id in ids)
            {

                //tasks.Add(Task.Run(() =>
                //{
                var plant = (Plant)repository.GetById(id.id);
                Console.WriteLine(string.Format("Asserting {0} on thread {1}", id.name, Thread.CurrentThread.ManagedThreadId));

                Assert.AreEqual(id.name, plant.State.Name);
                //}));

            }

            var newRepository = new GSRepository(
                    Kernel.Get<IStoreEvents>(),
                    Kernel.Get<IDetectConflicts>(),
                    Kernel.Get<IAggregateFactory>(),
                    Kernel.Get<IUIPersistence>(),
                    Kernel.Get<IPersistSyncStreams>()
                );
            foreach (var id in ids)
            {

                //tasks.Add(Task.Run(() =>
                //{
                var plant = (Plant)newRepository.GetById(id.id);
                Console.WriteLine(string.Format("Asserting from newRepository {0} on thread {1}", id.name, Thread.CurrentThread.ManagedThreadId));

                Assert.AreEqual(id.name, plant.State.Name);
                //}));

            }

            //Task.WaitAll(tasks.ToArray());


        }
    }
}
