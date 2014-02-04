
using Growthstories.Core;
using Growthstories.Sync;
using Growthstories.UI.Services;
using Growthstories.UI.ViewModel;
using System;
namespace Growthstories.UI.WindowsPhone
{

    public class BootstrapProduction : Bootstrap
    {



        public BootstrapProduction(App app)
            : base(app)
        {

        }


        protected override Uri BaseUri()
        {
            return new Uri("https://gs-prod.appspot.com");
        }


        protected override void AppConfiguration()
        {
            Bind<IIAPService>().To<GSIAP>().InSingletonScope();
            Bind<IScheduleService>().To<GardenScheduler>()
                .InSingletonScope()
                .WithConstructorArgument(new TimeSpan(0, 0, 60));

            Bind<ISynchronizer>().To<AutoSyncingSynchronizer>();
        }
      

    }
}
