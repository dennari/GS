using EventStore;
using EventStore.Persistence;
using EventStore.Persistence.SqlPersistence;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Growthstories.UI.Persistence;
using Growthstories.UI.ViewModel;
using Ninject;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.DomainTests
{
    public class TestAppViewModel : AppViewModel
    {
        protected IKernel Kernel;


        public TestAppViewModel(
            IMutableDependencyResolver resolver,
            IUserService context,
            IDispatchCommands handler,
            IGSRepository repository,
            ITransportEvents transporter,
            IUIPersistence uiPersistence,
            IIAPService iiapService,
            IScheduleService scheduler,
            ISynchronizer synchronizer,
            IRequestFactory requestFactory,
            IRoutingState router,
            IMessageBus bus
         )
            : base(
                resolver,
                context,
                handler,
                repository,
                transporter,
                uiPersistence,
                iiapService,
                scheduler,
                synchronizer,
                requestFactory,
                router,
                bus
                )
        {


            //TestUtils.WaitForTask(this.Initialize());

        }




    }

    public class StagingAppViewModel : AppViewModel
    {
        protected IKernel Kernel;


        public StagingAppViewModel(
           IMutableDependencyResolver resolver,
           IUserService context,
           IDispatchCommands handler,
           IGSRepository repository,
           ITransportEvents transporter,
           IUIPersistence uiPersistence,
           IIAPService iiapService,
           IScheduleService scheduler,
           ISynchronizer synchronizer,
           IRequestFactory requestFactory,
           IRoutingState router,
           IMessageBus bus
        )
            : base(
                resolver,
                context,
                handler,
                repository,
                transporter,
                uiPersistence,
                iiapService,
                scheduler,
                synchronizer,
                requestFactory,
                router,
                bus
                )
        {


            //this.Model = (GSApp)Kernel.Get<IDispatchCommands>().Handle(new CreateGSApp());
            //this.User = Context.CurrentUser;
        }


        protected override void ClearDB()
        {
            //base.ClearDB();
            var db = Kernel.Get<IPersistSyncStreams>() as SQLitePersistenceEngine;
            if (db != null)
                db.ReInitialize();
            var db2 = Kernel.Get<IUIPersistence>() as SQLiteUIPersistence;
            if (db2 != null)
                db2.ReInitialize();

            var repo = Repository as GSRepository;
            if (repo != null)
            {
                repo.ClearCaches();
            }
            var pipelineHook = Kernel.Get<OptimisticPipelineHook>();
            pipelineHook.Dispose();
        }



    }
}
