using Growthstories.Configuration;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using ReactiveUI;

namespace Growthstories.DomainTests
{
    public class TestModule : BaseSetup
    {


        public override void Load()
        {

            Bind<IGSAppViewModel, IScreen>().To<TestAppViewModel>();
            Bind<StagingAppViewModel>().To<StagingAppViewModel>();


            base.Load();
        }


        protected override void FileSystemConfiguration()
        {
            Bind<IPhotoHandler>().To<PhotoHandler>();

        }


    }
}


