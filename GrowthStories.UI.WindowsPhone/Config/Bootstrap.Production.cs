
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


        protected override void HandleUnhandledExceptions(object sender, System.Windows.ApplicationUnhandledExceptionEventArgs ee)
        {
            // tries to log
            base.HandleUnhandledExceptions(sender, ee);
            // don't crash
            ee.Handled = true;
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
