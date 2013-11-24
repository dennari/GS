﻿using Growthstories.Core;
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

        public Task AuthorizeUser()
        {
            return Task.FromResult(0);
        }

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
    }

}
