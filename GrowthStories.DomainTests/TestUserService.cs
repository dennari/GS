using Growthstories.Core;
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

        }

        public IAuthUser CurrentUser { get; private set; }



        public void SetupCurrentUser(IAuthUser user)
        {
            if (user != null)
            {
                CurrentUser = user;
            }
        }


        public Tuple<IAuthUser, IAggregateCommand[]> GetNewUserCommands()
        {

            IAuthUser authUser = new AuthUser()
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
            return Tuple.Create(authUser, new IAggregateCommand[0]);
        }


        public Task<IAuthResponse> AuthorizeUser()
        {
            return Task.FromResult<IAuthResponse>(new AuthResponse()
            {
                AuthToken = new AuthToken("hjkjh", 3600, "fghfgh"),
                StatusCode = GSStatusCode.OK
            });
        }


        public Task<IAuthResponse> AuthorizeUser(string email, string password)
        {
            //throw new NotImplementedException();
            var r = new AuthResponse()
            {
                AuthToken = new AuthToken("", 0, "") { },
                StatusCode = GSStatusCode.OK,
                StatusDescription = "OK"
            };
            return Task.FromResult((IAuthResponse)r);
        }


        public void ClearAuthorization()
        {
            //throw new NotImplementedException();
        }
    }

}
