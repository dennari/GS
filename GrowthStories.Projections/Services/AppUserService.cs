using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Growthstories.UI.Services
{

    public class AppUserService : IUserService
    {

        private readonly ITransportEvents Transporter;
        private readonly IDispatchCommands Handler;


        public AppUserService(
            ITransportEvents transporter,
            IDispatchCommands handler
            )
        {
            this.Transporter = transporter;
            this.Handler = handler;
        }


        //protected IAuthUser _OCurrentUser;
        protected IAuthUser _CurrentUser;
        public IAuthUser CurrentUser { get { return _CurrentUser; } private set { _CurrentUser = value; } }



        public void SetupCurrentUser(IAuthUser user)
        {
            _CurrentUser = user;
            if (user.Password == null || user.Password.Length == 0)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            if (user.AccessToken != null)
            {
                Transporter.AuthToken = user;
            }
        }

        public void ClearAuthorization()
        {
            if (CurrentUser == null)
                return;

            CurrentUser = new AuthUser()
            {
                Id = CurrentUser.Id,
                GardenId = CurrentUser.GardenId,
                Username = CurrentUser.Username,
                Password = CurrentUser.Password,
                Email = CurrentUser.Email,
                IsRegistered = CurrentUser.IsRegistered,
                IsCollaborator = CurrentUser.IsCollaborator
            };

            Transporter.AuthToken = null;

        }


        public Tuple<IAuthUser, IAggregateCommand[]> GetNewUserCommands()
        {
            var userId = Guid.NewGuid();
            var gardenId = Guid.NewGuid();
            var password = Guid.NewGuid().ToString();

            var commands = new IAggregateCommand[4];

            // TODO:
            // change unregpassword to randomly generated one
            // -- JOJ 4.1.2014
            //

            var u = new CreateUser(userId, AuthUser.UnregUsername, password,
                string.Format("{0}{1}@growthstories.com", AuthUser.UnregEmailPrefix, Guid.NewGuid()));
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


        // Tries to obtain authorization token for user
        // this will not anymore create (push) a new user
        // 
        // See AppViewModel.TryGetAuthorized()
        //
        public Task<IAuthResponse> AuthorizeUser()
        {
            if (CurrentUser == null)
            {
                throw new InvalidOperationException("CurrentUser has to be set before authorizing.");
            }

            return Task.Run(async () =>
            {
                var authResponse = await AuthorizeUser(CurrentUser.Email, CurrentUser.Password);

                if (authResponse.StatusCode != GSStatusCode.OK)
                {
                    return authResponse;
                }

                _CurrentUser.AccessToken = authResponse.AuthToken.AccessToken;
                _CurrentUser.ExpiresIn = authResponse.AuthToken.ExpiresIn;
                _CurrentUser.RefreshToken = authResponse.AuthToken.RefreshToken;

                Handler.Handle(new SetAuthToken(authResponse.AuthToken));

                Transporter.AuthToken = authResponse.AuthToken;
                return authResponse;
            });
        }


        public async Task<IAuthResponse> AuthorizeUser(string email, string password)
        {
            var authResponse = await Transporter.RequestAuthAsync(email, password);

            if (authResponse.StatusCode == GSStatusCode.OK)
            {
                Handler.Handle(new SetAuthToken(authResponse.AuthToken));
                Transporter.AuthToken = authResponse.AuthToken;
            }

            return authResponse;
        }

    }
}

