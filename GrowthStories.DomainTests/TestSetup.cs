using Growthstories.Sync;

namespace Growthstories.DomainTests
{
    public class TestModule : BaseSetup
    {



        protected override void FileSystemConfiguration()
        {
            Bind<IPhotoHandler>().To<PhotoHandler>();

        }


    }
}


