using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class FakeUserService : IUserService
    {
        private User u;

        public static Guid FakeUserId = Guid.Parse("12000000-0000-0000-0000-000000000000");
        public static Guid FakeUserGardenId = Guid.Parse("11100000-0000-0000-0000-000000000000");
        //public static Guid FakeUserId = Guid.NewGuid();
        //public static Guid FakeUserGardenId = Guid.NewGuid();


        private readonly IGSRepository Store;
        private readonly IAggregateFactory Factory;
        private readonly IAuthTokenService AuthService;

        public FakeUserService(IGSRepository store, IAggregateFactory factory, IAuthTokenService authService)
        {
            this.Store = store;
            this.Factory = factory;
            this.AuthService = authService;
        }

        //public FakeUserService(IGSRepository store, IAggregateFactory factory)
        //{
        //    this.Store = store;
        //    this.Factory = factory;
        //    //this.AuthService = authService;
        //}

        public IAuthUser CurrentUser
        {
            get
            {
                if (u == null)
                    EnsureCurrenUser();
                return u.State;
            }

        }

        public void EnsureCurrenUser()
        {
            u = Factory.Build<User>();
            Store.PlayById(u, FakeUserId);
            if (u.Version == 0)
            {
                u.Handle(new CreateUser(FakeUserId, "Fakename", "1234", "in@the.net"));
                u.Handle(new AddGarden(FakeUserId, FakeUserGardenId));

                var g = Factory.Build<Garden>();
                g.Handle(new CreateGarden(FakeUserGardenId)
                {
                    UserId = FakeUserId
                });

                Store.Save(u);
                Store.Save(g);

            }
        }

        public Task TryAuth()
        {
            return Task.Run(async () =>
            {
                var auth = await AuthService.GetAuthToken(CurrentUser.Username, CurrentUser.Password);
                //if(auth)
                u.Handle(new SetAuthToken(u.Id, auth));
                Store.Save(u);
            });
        }
    }
}
