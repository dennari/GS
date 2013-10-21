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
using EventStore.Persistence;
using Growthstories.UI.ViewModel;
using ReactiveUI;
namespace Growthstories.UI.Services
{

    public class AppUserService : IUserService
    {
        //private User u;
        private IAuthUser AuthUser;
        private readonly ITransportEvents Transporter;
        private readonly IMessageBus Bus;
        private readonly IDispatchCommands Handler;
        private IRequestFactory RequestFactory;

        public AppUserService(
            IMessageBus bus,
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IDispatchCommands handler
            )
        {

            this.Transporter = transporter;
            this.Bus = bus;
            this.Handler = handler;
            this.RequestFactory = requestFactory;
        }


        public IAuthUser CurrentUser
        {
            get
            {

                return AuthUser;
            }

        }



        public void SetupCurrentUser(AppViewModel app)
        {
            //u = Factory.Build<User>();
            //Repository.PlayById(u, FakeUserId);

            //if (u.Version == 0)
            //{
            //    u.Handle(new CreateUser(FakeUserId, "Fakename", "1234", "in@the.net"));
            //    Repository.Save(u);
            //    var commits = SyncPersistence.GetUnsynchronizedCommits().GroupBy(x => x.StreamId).ToArray();
            //    if (commits.Length != 1 || commits[0].Key != u.Id)
            //        throw new InvalidOperationException("UserCreated Event needs to be synchronized alone");
            //    this.UserCreateCommit = commits[0];
            //}


            if (app.Model.Version == 0 || app.Model.State.User == null)
            {

                var u = new CreateUser(UserState.UnregUserId, "UnregUser", "UnregPassword", "unreg@user.net");
                var U = (User)Handler.Handle(u);
                Handler.Handle(new CreateGarden(UserState.UnregUserGardenId, UserState.UnregUserId));
                Handler.Handle(new AddGarden(UserState.UnregUserId, UserState.UnregUserGardenId));
                Handler.Handle(new AssignAppUser(UserState.UnregUserId, u.Username, u.Password, u.Email)
                {
                    UserGardenId = UserState.UnregUserGardenId,
                    UserVersion = 0
                });
                //app.CurrentUserState = U.State;

                //Repository.Save(app);

            }

            AuthUser = app.Model.State.User;
            if (AuthUser == null)
                throw new InvalidOperationException("Can't set AppUser");

            // try to authorize
            //Task.Run(async () => await AuthorizeUser());
        }


        public Task AuthorizeUser()
        {

            return Task.Run(async () =>
            {

                var pushResp = await Transporter.PushAsync(RequestFactory.CreateUserSyncRequest(AuthUser.Id));
                if (pushResp.StatusCode != GSStatusCode.OK)
                    throw new InvalidOperationException("Can't synchronize user");
                //if (pushReq.Streams.Count > 1 || pushReq.Streams.First().StreamId != AuthUser.Id)
                //    throw new InvalidOperationException("Can't auth user");



                var authResponse = await Transporter.RequestAuthAsync(AuthUser.Username, AuthUser.Password);
                if (authResponse.StatusCode != GSStatusCode.OK)
                    throw new InvalidOperationException(string.Format("Can't authorize user {0}", AuthUser.Username));

                Bus.SendCommand(new SetAuthToken(authResponse.AuthToken));
                Transporter.AuthToken = authResponse.AuthToken;
                //u.State.Apply(new AuthTokenSet(new SetAuthToken(u.Id, authResponse.AuthToken)));
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

