using CommonDomain;
using Growthstories.Core;
using System;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public interface IUserService
    {
        IAuthUser CurrentUser { get; }

        Tuple<IAuthUser, IAggregateCommand[]> GetNewUserCommands();

        Task<IAuthResponse> AuthorizeUser();
        Task<IAuthResponse> AuthorizeUser(string email, string password);

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


        public IAuthUser SetupNewUser()
        {
            throw new System.NotImplementedException();
        }


        public Tuple<IAuthUser, IAggregateCommand[]> GetNewUserCommands()
        {
            throw new NotImplementedException();
        }


        Task<IAuthResponse> IUserService.AuthorizeUser()
        {
            throw new NotImplementedException();
        }

        public Task<IAuthResponse> AuthorizeUser(string email, string password)
        {
            throw new NotImplementedException();
        }
    }

}
