using CommonDomain;
using Growthstories.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public class FakeUserService : IUserService
    {
        private readonly User u = new User();
        public static Guid FakeUserId = Guid.Parse("10000000-0000-0000-0000-000000000000");

        public FakeUserService()
        {
            u.Create(FakeUserId);
        }


        public IAuthUser CurrentUser
        {
            get
            {
                return u.State;
            }
            set
            {
                //u.ApplyState()
            }
        }
    }
}
