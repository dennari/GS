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

        private readonly ISynchronizerService SyncService;
        private readonly ITransportEvents Transporter;
        private readonly IDispatchCommands Handler;
        private IRequestFactory RequestFactory;

        public AppUserService(
            ISynchronizerService syncService,
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            IDispatchCommands handler
            )
        {

            this.Transporter = transporter;
            this.SyncService = syncService;
            this.Handler = handler;
            this.RequestFactory = requestFactory;
        }


        protected IAuthUser _OCurrentUser;
        protected AuthUser _CurrentUser;
        public IAuthUser CurrentUser { get { return _OCurrentUser ?? _CurrentUser; } }



        public void SetupCurrentUser(IAuthUser user)
        {
            if (CurrentUser != null)
                return;
            if (user != null)
                _OCurrentUser = user;
            else
            {

                var u = new CreateUser(UserState.UnregUserId, "UnregUser", "unregpassword", string.Format("unreg{0}@tussu.org", DateTime.UtcNow.Ticks));
                var U = (User)Handler.Handle(u);
                Handler.Handle(new CreateGarden(UserState.UnregUserGardenId, UserState.UnregUserId));
                Handler.Handle(new AddGarden(UserState.UnregUserId, UserState.UnregUserGardenId));
                Handler.Handle(new AssignAppUser(UserState.UnregUserId, u.Username, u.Password, u.Email)
                {
                    UserGardenId = UserState.UnregUserGardenId,
                    UserVersion = 0
                });

                _CurrentUser = new AuthUser()
                {
                    Id = u.AggregateId,
                    GardenId = UserState.UnregUserGardenId,
                    Username = u.Username,
                    Password = u.Password,
                    Email = u.Email
                };

            }

        }


        public Task AuthorizeUser()
        {

            if (CurrentUser == null)
            {
                throw new InvalidOperationException("CurrentUser has to be set before authorizing.");
            }

            return Task.Run(async () =>
            {

                var s = SyncService.Synchronize(RequestFactory.CreatePullRequest(null), RequestFactory.CreateUserSyncRequest(CurrentUser.Id));
                int counter = 0;
                ISyncPushResponse pushResp = null;
                while (counter < 3)
                {
                    pushResp = await s.Push();
                    if (pushResp.StatusCode == GSStatusCode.OK)
                        break;
                    await Task.Delay(2000);
                    //throw new InvalidOperationException("Can't synchronize user");
                    counter++;
                }
                if (pushResp.StatusCode != GSStatusCode.OK)
                    throw new InvalidOperationException("Can't synchronize user");

                Handler.Handle(new Push(s));
                //if (pushReq.Streams.Count > 1 || pushReq.Streams.First().StreamId != AuthUser.Id)
                //    throw new InvalidOperationException("Can't auth user");



                IAuthResponse authResponse = null;
                counter = 0;
                while (counter < 3)
                {
                    authResponse = await Transporter.RequestAuthAsync(CurrentUser.Email, CurrentUser.Password);
                    if (authResponse.StatusCode == GSStatusCode.OK)
                        break;
                    await Task.Delay(2000);
                    //throw new InvalidOperationException("Can't synchronize user");
                    counter++;
                }

                if (authResponse.StatusCode != GSStatusCode.OK)
                    throw new InvalidOperationException(string.Format("Can't authorize user {0}", CurrentUser.Username));

                _CurrentUser.AccessToken = authResponse.AuthToken.AccessToken;
                _CurrentUser.ExpiresIn = authResponse.AuthToken.ExpiresIn;
                _CurrentUser.RefreshToken = authResponse.AuthToken.RefreshToken;

                Handler.Handle(new SetAuthToken(authResponse.AuthToken));

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

