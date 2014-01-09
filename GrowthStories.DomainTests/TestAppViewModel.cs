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


        public TestAppViewModel(IKernel kernel)
            : base()
        {
            Kernel = kernel;
            //Kernel.Bind<IScreen>().ToConstant(this);
            //Kernel.Bind<IRoutingState>().ToConstant(this.Router);
            this.Bus = kernel.Get<IMessageBus>();

            TestUtils.WaitForTask(this.Initialize());

            //this.Model = (GSApp)Kernel.Get<IDispatchCommands>().Handle(new CreateGSApp());
            //this.User = Context.CurrentUser;
        }

        //public new Task<IAuthUser> Initialize()
        //{

        //    return null;
        //}

    }

    public class StagingAppViewModel : AppViewModel
    {


        public StagingAppViewModel(IKernel kernel)
            : base()
        {
            Kernel = kernel;
            Kernel.Bind<IScreen>().ToConstant(this);
            Kernel.Bind<IRoutingState>().ToConstant(this.Router);
            this.Bus = kernel.Get<IMessageBus>();
            //Initialize();

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
