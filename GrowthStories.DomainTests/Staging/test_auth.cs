using System;
using System.Linq;

using NUnit.Framework;
using Growthstories.Domain.Messaging;
using Ninject;
using Growthstories.Core;
using CommonDomain.Persistence;
using Growthstories.Sync;
using Ninject.Parameters;
using SimpleTesting;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.Collections.Generic;
using Growthstories.Domain.Entities;
using CommonDomain;
using EventStore;

using CommonDomain.Persistence.EventStore;
using EventStore.Dispatcher;
using Growthstories.Domain;
using EventStore.Logging;
using Growthstories.Projections;
using System.Text;

namespace Growthstories.DomainTests
{
    public class AuthTest
    {

        IKernel kernel;
        [SetUp]
        public void SetUp()
        {
            if (kernel != null)
                kernel.Dispose();
            kernel = new StandardKernel(new StagingModule());
            Log.Info("-----------------------------------------------------------------------------");
        }
        private ILog Log = new LogTo4Net(typeof(AuthTest));

        public T Get<T>() { return kernel.Get<T>(); }
        public IDispatchCommands Handler { get { return Get<IDispatchCommands>(); } }
        public IAsyncDispatchCommits AsyncDispatcher { get { return Get<IAsyncDispatchCommits>(); } }


        public ISynchronizerService Synchronizer { get { return Get<ISynchronizerService>(); } }
        public IStoreSyncHeads SyncStore { get { return Get<IStoreSyncHeads>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }

        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IGSRepository Repository { get { return Get<IGSRepository>(); } }
        public IStoreEvents EventStore { get { return Get<IStoreEvents>(); } }


        public IDispatchCommits Dispatcher { get { return Get<IDispatchCommits>(); } }


        public IUserService UserService { get { return Get<IUserService>(); } }

        public IHttpClient HttpClient { get { return kernel.Get<IHttpClient>(); } }
        public CompareObjects Comparer { get { return new CompareObjects(); } }


        private static Guid SynchronizerId = Guid.NewGuid();
        private static bool SynchronizerCreated = false;

        protected void EnsureSynchronizer()
        {
            if (SynchronizerCreated)
                return;
            Handler.Handle<Synchronizer, CreateSynchronizer>(new CreateSynchronizer(SynchronizerId));
            SynchronizerCreated = true;
        }

        protected async Task<IList<ISyncRequest>> Sync()
        {
            EnsureSynchronizer();
            var r = (IList<ISyncRequest>)await Handler.HandlerHandleAsync<Synchronizer, Synchronize>(new Synchronize(SynchronizerId));
            return r;
        }

        protected Task<IAuthTokenResponse> _Auth(CreateUser uCmd)
        {
            return Task.Run<IAuthTokenResponse>(async () =>
            {
                var u = Handler.Handle<User, CreateUser>(uCmd);
                //User u = Repository.GetById<User>(uCmd.EntityId);
                UserService.CurrentUser = u.State;
                //var task = Sync().ContinueWith(async (prev) =>
                //{
                //    await this.AsyncDispatcher.DispatchAsync();
                //});

                var r = await Sync();
                var auth = await kernel.Get<AuthTokenService>().GetAuthToken(uCmd.Username, uCmd.Password);
                u.Handle(new SetAuthToken(u.Id, auth));
                return auth;
            });
        }

        [Test]
        public async void TestAuth()
        {

            Log.Info("TestAuth");
            var uCmd = new CreateUser(Guid.NewGuid(), "Alice", "swordfish", "alice@wonderland.net");

            var auth = await _Auth(uCmd);

            Assert.IsNotNullOrEmpty(auth.AccessToken);
            Assert.IsNotNullOrEmpty(auth.RefreshToken);
            Assert.Greater(auth.ExpiresIn, 0);
            //Assert.IsNull(auth.ExpiresIn);


        }

        protected string randomize(string i)
        {
            //var b = new StringBuilder(i);
            //b.Append(Guid.NewGuid().ToString().Substring(0, 4));
            return i + Guid.NewGuid().ToString().Substring(0, 4);
        }

        [Test]
        public async void TestAddRelationship()
        {

            Log.Info("TestAddRelationship");
            var uCmd = new CreateUser(Guid.NewGuid(), randomize("Alice"), randomize("swordfish"), randomize("alice") + "@wonderland.net");

            var auth = await _Auth(uCmd);

            var fCmd = new CreateUser(Guid.NewGuid(), randomize("Bob"), randomize("swordfish"), randomize("bob") + "@wonderland.net");
            var Bob = Handler.Handle<User, CreateUser>(fCmd);

            var r = await Sync();

            var relationshipCmd = new BecomeFollower(uCmd.EntityId, fCmd.EntityId);
            Handler.Handle<User, BecomeFollower>(relationshipCmd);

            r = await Sync();

        }





    }
}
