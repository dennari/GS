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


        //protected IAuthUser _OCurrentUser;
        protected IAuthUser _CurrentUser;
        public IAuthUser CurrentUser { get { return _CurrentUser; } private set { _CurrentUser = value; } }



        public void SetupCurrentUser(IAuthUser user)
        {
            _CurrentUser = user;
            if (user.AccessToken != null)
            {
                Transporter.AuthToken = user;
            }
        }


        public Tuple<IAuthUser, IAggregateCommand[]> GetNewUserCommands()
        {
            var userId = Guid.NewGuid();
            var gardenId = Guid.NewGuid();

            var commands = new IAggregateCommand[4];

            var u = new CreateUser(userId, "UnregUser", "unregpassword", string.Format("{0}{1}@growthstories.com", AuthUser.UnregEmailPrefix, Guid.NewGuid()));
            commands[0] = u;
            commands[1] = new CreateGarden(gardenId, userId);
            commands[2] = new AddGarden(userId, gardenId);
            commands[3] = new AssignAppUser(userId, u.Username, u.Password, u.Email)
            {
                UserGardenId = gardenId,
                UserVersion = 0
            };

            IAuthUser authUser = new AuthUser()
            {
                Id = userId,
                GardenId = gardenId,
                Username = u.Username,
                Password = u.Password,
                Email = u.Email
            };

            return Tuple.Create(authUser, commands);

        }

        public Task<IAuthResponse> AuthorizeUser()
        {

            if (CurrentUser == null)
            {
                throw new InvalidOperationException("CurrentUser has to be set before authorizing.");
            }

            return Task.Run(async () =>
            {

                var s = SyncService.Synchronize(RequestFactory.CreatePullRequest(null), RequestFactory.CreateUserSyncRequest(CurrentUser.Id));
                //int counter = 0;
                ISyncPushResponse pushResp = await s.Push();

                if (pushResp.StatusCode != GSStatusCode.OK)
                    throw new InvalidOperationException("Can't synchronize user");

                Handler.Handle(new Push(s));
                //if (pushReq.Streams.Count > 1 || pushReq.Streams.First().StreamId != AuthUser.Id)
                //    throw new InvalidOperationException("Can't auth user");


                var authResponse = await AuthorizeUser(CurrentUser.Email, CurrentUser.Password);

                _CurrentUser.AccessToken = authResponse.AuthToken.AccessToken;
                _CurrentUser.ExpiresIn = authResponse.AuthToken.ExpiresIn;
                _CurrentUser.RefreshToken = authResponse.AuthToken.RefreshToken;

                Handler.Handle(new SetAuthToken(authResponse.AuthToken));

                Transporter.AuthToken = authResponse.AuthToken;
                //u.State.Apply(new AuthTokenSet(new SetAuthToken(u.Id, authResponse.AuthToken)));
                return authResponse;
            });

        }

        public async Task<IAuthResponse> AuthorizeUser(string email, string password)
        {


            var authResponse = await Transporter.RequestAuthAsync(email, password);

            if (authResponse.StatusCode != GSStatusCode.OK)
                throw new InvalidOperationException(string.Format("Can't authorize user {0}", email));


            Handler.Handle(new SetAuthToken(authResponse.AuthToken));

            Transporter.AuthToken = authResponse.AuthToken;
            return authResponse;


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

