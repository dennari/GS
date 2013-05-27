using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using CommonDomain.Persistence.EventStore;
using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Messaging;
using Growthstories.Domain.Services;
using Growthstories.Sync;
using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.DomainTests
{
    public static class TestExtensions
    {
        public static IKernel WireUp(this IKernel kernel)
        {
            kernel.Bind<IStoreSyncHeads>().To<SynchronizerInMemoryPersistence>().InSingletonScope();
            kernel.Bind<IDispatchCommits>().To<SyncDispatcher>().InSingletonScope();

            kernel.Bind<IStoreEvents>().ToMethod(_ =>
            {
                return Wireup.Init()
                .LogToOutputWindow()
                .UsingSerializingInMemoryPersistence()
                    //.HookIntoPipelineUsing(new SynchronizerPipelineHook())
                    //.UsingInMemoryPersistence()
                .UsingSynchronousDispatchScheduler()
                .DispatchTo(_.Kernel.Get<IDispatchCommits>())
                .Build();
            });


            kernel.Bind<IRepository>().ToConstructor(aa => new EventStoreRepository(
                aa.Context.GetParameter<IStoreEvents>("store") ?? aa.Inject<IStoreEvents>(),
                aa.Inject<IConstructAggregates>(),
                aa.Inject<IDetectConflicts>())
            );
            kernel.Bind<ICommandHandler<ICommand>>().To<CommandHandler>();
            kernel.Bind<IConstructAggregates>().To<AggregateFactory>().InSingletonScope();
            kernel.Bind<IDetectConflicts>().To<ConflictDetector>().InSingletonScope();
            return kernel;
        }

        public static IKernel WireUp2(this IKernel kernel)
        {
            kernel.Bind<IStoreSyncHeads>().To<SynchronizerInMemoryPersistence>().InSingletonScope();
            kernel.Bind<IDispatchCommits>().To<SyncDispatcher>().InSingletonScope();

            kernel.Bind<IStoreEvents>().ToMethod(_ =>
            {
                return Wireup.Init()
                .LogToOutputWindow()
                .UsingSerializingInMemoryPersistence()
                    //.HookIntoPipelineUsing(new SynchronizerPipelineHook())
                    //.UsingInMemoryPersistence()
                .UsingSynchronousDispatchScheduler()
                .DispatchTo(_.Kernel.Get<IDispatchCommits>())
                .Build();
            });


            kernel.Bind<IRepository>().ToConstructor(aa => new EventStoreRepository(
                aa.Inject<IStoreEvents>(),
                aa.Inject<IConstructAggregates>(),
                aa.Inject<IDetectConflicts>())
            );
            kernel.Bind<ICommandHandler<ICommand>>().To<CommandHandler>();
            kernel.Bind<IConstructAggregates>().To<AggregateFactory>().InSingletonScope();
            kernel.Bind<IDetectConflicts>().To<ConflictDetector>().InSingletonScope();
            return kernel;
        }
    }
}


