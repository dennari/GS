using CommonDomain;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public interface IUserService
    {
        IAuthUser CurrentUser { get; }

        Task TryAuth();
    }

    public class NullUserService : IUserService
    {

        public IAuthUser CurrentUser
        {
            get { throw new System.NotImplementedException(); }
        }

        public Task TryAuth()
        {
            throw new System.NotImplementedException();
        }
    }

}
