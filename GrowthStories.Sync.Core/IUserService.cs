using CommonDomain;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public interface IUserService
    {
        IAuthUser CurrentUser { get; }



        Task AuthorizeUser();

        void SetupCurrentUser(IAuthUser user);
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


        public void SetupCurrentUser(IAuthUser user)
        {
            throw new System.NotImplementedException();
        }
    }

}
