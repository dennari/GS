using Growthstories.Domain.Entities;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.DomainTests
{
    public class TestUserService : IUserService
    {

        public TestUserService()
        {
            _CurrentUser = new AuthUser()
            {
                Id = Guid.NewGuid(),
                GardenId = Guid.NewGuid(),
                Username = "testUser",
                Password = "testPassword",
                Email = "test@mail.net",
                AccessToken = "sdfsd",
                RefreshToken = "sdsdf",
                ExpiresIn = 23
            };
        }

        protected AuthUser _CurrentUser;
        public IAuthUser CurrentUser
        {
            get { return _CurrentUser; }
        }

        public Task AuthorizeUser()
        {
            return Task.FromResult(0);
        }

        public void SetupCurrentUser(IAuthUser user)
        {
            //throw new NotImplementedException();
        }
    }

}
