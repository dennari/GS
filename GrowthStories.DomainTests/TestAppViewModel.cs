using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
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
            Kernel.Bind<IScreen>().ToConstant(this);
            Kernel.Bind<IRoutingState>().ToConstant(this.Router);
            this.Bus = kernel.Get<IMessageBus>();
            //Initialize();

            this.Model = (GSApp)Kernel.Get<IDispatchCommands>().Handle(new CreateGSApp());
            this.User = Context.CurrentUser;
        }

        public new Task<IAuthUser> Initialize()
        {

            //if (this.Model != null)
            //    return null;

            //return Task.Run(async () =>
            //{
            //    GSApp app = null;

            //    try
            //    {
            //        app = (GSApp)(await GetById(GSAppState.GSAppId));
            //    }
            //    catch (DomainError)
            //    {

            //    }

            //    if (app == null)
            //    {
            //        app = (GSApp)(await HandleCommand(new CreateGSApp()));
            //        //app = (GSApp)Handler.Handle(new CreateGSApp());
            //    }
            //    Context.SetupCurrentUser(app.State.User);

            //    this.Model = app;
            //    this.User = Context.CurrentUser;
            //    return this.User;
            //    //return app;
            //});
            return null;
        }

    }
}
