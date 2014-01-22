
using Growthstories.Core;
using Growthstories.Sync;
using Growthstories.UI.Services;
using Growthstories.UI.ViewModel;
using System;
namespace Growthstories.UI.WindowsPhone
{
    public class BootstrapProduction : Bootstrap
    {


        private const string BACKENDHOST = "dennari-macbook.lan";
        private const int BACKENDPORT = 8080;
        private const string LOGHOST = "dennari-macbook.lan";
        private const int LOGPORT = 2877;



        public BootstrapProduction(App app)
            : base(app)
        {

        }

        protected override void AppConfiguration()
        {
            Bind<IIAPService>().To<GSIAP>().InSingletonScope();
            Bind<IScheduleService>().To<GardenScheduler>()
                .InSingletonScope()
                .WithConstructorArgument(new TimeSpan(0, 0, 10));

            Bind<ISynchronizer>().To<AutoSyncingSynchronizer>();

        }


        protected override void HttpConfiguration(string host = "default.lan", int port = 80)
        {
            base.HttpConfiguration(BACKENDHOST, BACKENDPORT);
        }

    }
}
