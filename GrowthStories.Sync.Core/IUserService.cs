using CommonDomain;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public interface IUserService
    {
        IAuthUser CurrentUser { get; }

        Task AuthorizeUser();
    }

    public class NullUserService : IUserService
    {

        public IAuthUser CurrentUser
        {
            get { throw new System.NotImplementedException(); }
        }

        public Task AuthorizeUser()
        {
            throw new System.NotImplementedException();
        }
    }

}
