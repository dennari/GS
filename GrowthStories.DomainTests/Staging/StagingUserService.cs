using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace Growthstories.DomainTests.Staging
{

    public class StagingUserService : IUserService
    {
        private User u;

        public static Guid FakeUserId = Guid.Parse("10000000-0000-0000-0000-000000000000");
        //public static Guid FakeUserGardenId = Guid.Parse("11100000-0000-0000-0000-000000000000");
        //public static Guid FakeUserId = Guid.NewGuid();
        //public static Guid FakeUserGardenId = Guid.NewGuid();


        private readonly IGSRepository Repository;
        private readonly GSEventStore Store;
        private readonly IAggregateFactory Factory;
        private readonly ITransportEvents Transporter;
        private readonly IRequestFactory RequestFactory;
        private IGrouping<Guid, EventStore.Commit> UserCreateCommit;

        public StagingUserService(
            IGSRepository store,
            IAggregateFactory factory,
            ITransportEvents transporter,
            IRequestFactory requestFactory
            )
        {
            this.Repository = store;
            this.Store = (Repository as GSRepository).EventStore;
            this.Factory = factory;
            this.Transporter = transporter;
            this.RequestFactory = requestFactory;
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

        protected void EnsureCurrenUser()
        {
            u = Factory.Build<User>();
            Repository.PlayById(u, FakeUserId);

            if (u.Version == 0)
            {
                u.Handle(new CreateUser(FakeUserId, "Fakename", "1234", "in@the.net"));
                Repository.Save(u);
                var commits = Store.MoreAdvanced.GetUnsynchronizedCommits().GroupBy(x => x.StreamId).ToArray();
                if (commits.Length != 1 || commits[0].Key != u.Id)
                    throw new InvalidOperationException("UserCreated Event needs to be synchronized alone");
                this.UserCreateCommit = commits[0];
            }

        }


        public Task AuthorizeUser()
        {

            return Task.Run(async () =>
            {
                var pushResponse = await Transporter.PushAsync(
                    RequestFactory.CreatePushRequest(
                        new ISyncEventStream[] { new SyncEventStream(UserCreateCommit, Store) }));


                if (pushResponse.StatusCode != GSStatusCode.OK)
                    throw new InvalidOperationException("Can't create user");

                Store.MoreAdvanced.MarkCommitAsSynchronized(UserCreateCommit.First());

                var authResponse = await Transporter.RequestAuthAsync(u.State.Username, u.State.Password);
                if (authResponse.StatusCode != GSStatusCode.OK)
                    throw new InvalidOperationException("Can't create user");

                Transporter.AuthToken = authResponse.AuthToken;
                u.State.Apply(new AuthTokenSet(new SetAuthToken(u.Id, authResponse.AuthToken)));
            });

        }

        //public Task TryAuth()
        //{
        //    return Task.Run(async () =>
        //    {
        //        var auth = await AuthService.GetAuthToken(CurrentUser.Username, CurrentUser.Password);

        //        u.State.Apply(new AuthTokenSet(new SetAuthToken(u.Id, auth)));

        //    });
        //}
    }
}

