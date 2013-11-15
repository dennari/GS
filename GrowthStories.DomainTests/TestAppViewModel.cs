using Growthstories.Core;
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
        }



    }
}
