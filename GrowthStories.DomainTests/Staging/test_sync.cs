using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

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
using Growthstories.UI;
using System.Text;
using Growthstories.UI.ViewModel;
using System.IO;
using EventStore.Persistence;



namespace Growthstories.DomainTests
{
    public class SyncTest : StagingTestBase
    {


        private ILog Log = new LogToNLog(typeof(SyncTest));

        private IAuthToken RemoteAuth;


        public async Task SetupAsync()
        {


            this.Ctx = App.Context.CurrentUser;
            Assert.IsNotNull(Ctx);
            Assert.IsNotNullOrEmpty(Ctx.Username);

            await HttpClient.SendAsync(HttpClient.CreateClearDBRequest());
            //await Get<ISynchronizerService>().CreateUserAsync(Ctx.Id);


            await App.Context.AuthorizeUser();
            //Ctx = App.Context.CurrentUser;

            Assert.IsNotNullOrEmpty(Ctx.AccessToken);
            Assert.IsNotNullOrEmpty(Ctx.RefreshToken);
            Assert.Greater(Ctx.ExpiresIn, 0);

        }




        [Test]
        public async void RealTestSyncUserStream()
        {

            await TestSyncUserStream();

        }

        public async Task TestSyncUserStream()
        {

            await SetupAsync();

            var originalRemoteEvents = await CreateRemoteData();

            var remoteUser = (UserCreated)originalRemoteEvents[0];

            var listUsersResponse = await Transporter.ListUsersAsync(remoteUser.Username);

            var fetchedUser = listUsersResponse.Users[0];


            Bus.SendCommand(new CreateSyncStream(fetchedUser.AggregateId, StreamType.USER));

            var R2 = await App.Synchronize();
            SyncAssertions(R2);

            var remoteUserAggregate = (User)Repository.GetById(remoteUser.AggregateId);

            Assert.AreEqual(4, remoteUserAggregate.Version);
            Assert.AreEqual(remoteUser.Username, remoteUserAggregate.State.Username);
            Assert.AreEqual(1, remoteUserAggregate.State.Gardens.Count);
            Assert.AreEqual(originalRemoteEvents[1].EntityId.Value, remoteUserAggregate.State.Garden.Id);
            Assert.AreEqual(originalRemoteEvents[3].AggregateId, remoteUserAggregate.State.Garden.PlantIds[0]);

            var R3 = await App.Synchronize();
            SyncAssertions(R3);
            Assert.IsNull(R3.Pushes[0].Item2);

        }

        [Test]
        public async void RealTestAddUserViewModel()
        {




            await TestAddUserViewModel();

        }


        public async Task<List<IEvent>> TestAddUserViewModel()
        {
            await SetupAsync();
            var originalRemoteEvents = await CreateRemoteData();

            var remoteUser = (UserCreated)originalRemoteEvents[0];

            var remotePlant = (PlantCreated)originalRemoteEvents[3];

            var remoteComment = (PlantActionCreated)originalRemoteEvents[5];

            var lvm = new ListUsersViewModel(Get<ITransportEvents>(), Get<IGSRepository>(), App);
            //IUserListResponse R = null;
            var task = lvm.SearchResults.Take(1).GetAwaiter();
            //first.Subscribe(x => R = x);
            lvm.SearchCommand.Execute("Lauri");
            var R = await task;
            Assert.IsNotNull(R);
            Assert.IsTrue(R.Users.Count > 0);
            var lauri = R.Users[0];

            var task2 = lvm.SyncStreams.Take(1).GetAwaiter();

            lvm.UserSelectedCommand.Execute(lauri);

            var R2 = await task2;
            Assert.AreEqual(2, R2.Count);


            var fvm = (FriendsViewModel)App.Resolver.GetService(typeof(FriendsViewModel));

            var R3 = await App.Synchronize();
            SyncAssertions(R3);

            Assert.AreEqual(1, fvm.Friends.Count);
            var friend = fvm.Friends[0];
            Assert.AreEqual(remoteUser.AggregateId, friend.UserState.Id);
            Assert.AreEqual(remoteUser.Username, friend.UserState.Username);

            Assert.AreEqual(1, friend.Plants.Count);
            var plant = friend.Plants[0];

            Assert.AreEqual(remotePlant.AggregateId, plant.State.Id);
            Assert.AreEqual(remotePlant.Name, plant.State.Name);


            //var task3 = plant.Actions.ItemsAdded.Take(1).GetAwaiter();
            Assert.AreEqual(1, plant.Actions.Count);
            var action = plant.Actions[0];
            Assert.AreEqual(remoteComment.AggregateId, action.State.Id);
            Assert.AreEqual(remoteComment.Type, action.State.Type);
            Assert.AreEqual(remoteComment.Note, action.State.Note);


            var R4 = await App.Synchronize();
            SyncAssertions(R4);
            Assert.IsNull(R4.Pushes[0].Item2);

            return originalRemoteEvents;

        }





        [Test]
        public async void RealTestReadModel()
        {

            await TestSyncReadModel();

        }

        public async Task TestSyncReadModel()
        {

            var originalRemoteEvents = await TestAddUserViewModel();

            var remoteUser = (UserCreated)originalRemoteEvents[0];

            var remotePlant = (PlantCreated)originalRemoteEvents[3];

            var remoteComment = (PlantActionCreated)originalRemoteEvents[5];

            this.App = new TestAppViewModel(Kernel);
            this.Ctx = App.Context.CurrentUser;
            Assert.IsNotNull(Ctx);
            Assert.IsNotNullOrEmpty(Ctx.Username);

            var fvm = (FriendsViewModel)App.Resolver.GetService(typeof(FriendsViewModel));

            Assert.AreEqual(1, fvm.Friends.Count);
            var friend = fvm.Friends[0];
            Assert.AreEqual(remoteUser.AggregateId, friend.UserState.Id);
            Assert.AreEqual(remoteUser.Username, friend.UserState.Username);
        }


        [Test]
        public async void RealTestSyncConflict()
        {

            await TestSyncConflict();

        }

        public async Task TestSyncConflict()
        {

            await SetupAsync();

            var plant = new CreatePlant(Guid.NewGuid(), "Jare", Ctx.GardenId, Ctx.Id);
            Bus.SendCommand(App.SetIds(plant));


            var R4 = await App.Synchronize();
            SyncAssertions(R4);


            var setNameCmd = App.SetIds(new SetName(plant.AggregateId, "Sepi"));
            Bus.SendCommand(setNameCmd);


            var setNameEvent = new NameSet(setNameCmd)
            {
                AggregateVersion = 2,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            await PushAsAnother(Transporter.AuthToken, new IEvent[] { setNameEvent });

            var R5 = await App.Synchronize();
            SyncAssertions(R5);

        }




        [Test]
        public async void RealTestSyncPlantStream()
        {

            await TestSyncPlantStream();

        }

        public async Task TestSyncPlantStream()
        {

            await SetupAsync();

            var originalRemoteEvents = await CreateRemoteData();

            var remoteUser = (UserCreated)originalRemoteEvents[0];

            var remotePlant = (PlantCreated)originalRemoteEvents[3];

            var remoteComment = (PlantActionCreated)originalRemoteEvents[5];


            Bus.SendCommand(new CreateSyncStream(remotePlant.AggregateId, StreamType.PLANT, remoteUser.AggregateId));

            var R2 = await App.Synchronize();
            SyncAssertions(R2);

            var remotePlantAggregate = (Plant)Repository.GetById(remotePlant.AggregateId);

            Assert.AreEqual(2, remotePlantAggregate.Version);
            Assert.AreEqual(remotePlant.Name, remotePlantAggregate.State.Name);

            var remoteCommentAggregate = (PlantAction)Repository.GetById(remoteComment.AggregateId);

            Assert.AreEqual(1, remoteCommentAggregate.Version);
            Assert.AreEqual(remoteComment.Note, remoteCommentAggregate.State.Note);


            var R3 = await App.Synchronize();
            SyncAssertions(R3);
            Assert.IsNull(R3.Pushes[0].Item2);

        }



        [Test]
        public async void RealTestFriendship()
        {

            await TestFriendship();

        }


        public async Task<IAggregateCommand> TestFriendship()
        {

            await SetupAsync();

            var originalRemoteEvents = await CreateRemoteData();

            var remoteUser = (UserCreated)originalRemoteEvents[0];

            var relationshipCmd = App.SetIds(new BecomeFollower(Ctx.Id, remoteUser.AggregateId));
            Bus.SendCommand(relationshipCmd);

            var friendshipCmd = App.SetIds(new RequestCollaboration(Ctx.Id, remoteUser.AggregateId));
            Bus.SendCommand(friendshipCmd);


            var R = await App.Synchronize();
            SyncAssertions(R);




            return relationshipCmd;

        }


        protected UserCreated CreateUserFromName(string name)
        {
            return new UserCreated(new CreateUser(
                Guid.NewGuid(),
                name,
                randomize("swordfish"),
                randomize(name) + "@wonderland.net"))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };
        }



        protected async Task<List<IEvent>> CreateRemoteData()
        {
            // create remote data



            var users = ListOfNames().Select(x => CreateUserFromName(x.Trim())).ToArray();
            var remoteUser = users[0];

            var pushResp = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Events = Translator.Out(users),
                ClientDatabaseId = Guid.NewGuid()
            });
            Assert.AreEqual(GSStatusCode.OK, pushResp.StatusCode);

            var remoteGarden = new GardenCreated(App.SetIds(new CreateGarden(Guid.NewGuid(), remoteUser.AggregateId), null, remoteUser.AggregateId))
            {
                AggregateVersion = 2,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var remoteAddGarden = new GardenAdded(App.SetIds(new AddGarden(remoteUser.AggregateId, remoteGarden.EntityId.Value), null, remoteUser.AggregateId))
            {
                AggregateVersion = 3,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var remotePlant = new PlantCreated(App.SetIds(new CreatePlant(Guid.NewGuid(), "RemoteJare", remoteGarden.EntityId.Value, remoteUser.AggregateId), null, remoteUser.AggregateId))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var remotePlantProperty = new MarkedPlantPublic(App.SetIds(new MarkPlantPublic(remotePlant.AggregateId), null, remoteUser.AggregateId))
            {
                AggregateVersion = 2,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };


            var remoteAddPlant = new PlantAdded(App.SetIds(new AddPlant(remoteGarden.EntityId.Value, remotePlant.AggregateId, remoteUser.AggregateId, "RemoteJare"), null, remoteUser.AggregateId))
            {
                AggregateVersion = 4,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var remoteComment = new PlantActionCreated(
                App.SetIds(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        remoteUser.AggregateId,
                        remotePlant.AggregateId,
                        PlantActionType.COMMENTED,
                        "Hello remote world"),
                    null, remoteUser.AggregateId))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };



            var remoteEvents = new List<IEvent>() { 
                remoteGarden, 
                remoteAddGarden, 
                remotePlant, 
                remoteAddPlant,
                remoteComment,
                remotePlantProperty
            };





            var authResponse = await Transporter.RequestAuthAsync(remoteUser.Username, remoteUser.Password);

            Assert.AreEqual(authResponse.StatusCode, GSStatusCode.OK);
            this.RemoteAuth = authResponse.AuthToken;

            await PushAsAnother(RemoteAuth, remoteEvents);

            remoteEvents.Insert(0, remoteUser);

            return remoteEvents;
        }

        protected async Task PushAsAnother(IAuthToken anotherAuth, IEnumerable<IEvent> events)
        {
            var tmp = HttpClient.AuthToken;
            HttpClient.AuthToken = anotherAuth;

            var pushResp2 = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Events = Translator.Out(events).ToArray(),
                ClientDatabaseId = Guid.NewGuid()
            });

            Assert.AreEqual(GSStatusCode.OK, pushResp2.StatusCode);

            HttpClient.AuthToken = tmp;

        }

        protected string[] ListOfNames()
        {
            return @"Lauri  
Yadira  
Yukiko  
Nga  
Lashell  
Maryalice  
Danette  
Johnnie  
Sarah  
Maynard  
Raisa  
Stefany  
Lavinia  
Lynnette  
Kirby  
Johnette  
Peter  
Rudy  
Delilah  
Genie  
Ferne  
Fatimah  
Shannon  
Delbert  
Clarine  
Mavis  
Floyd  
Grisel  
Julieta  
Darlene  
Roscoe  
Johnna  
Willia  
Andra  
Ignacia  
Patricia  
Yasuko  
Reina  
Rochelle  
Elnora  
Maye  
Margarito  
Samella  
Omar  
Myrtice  
Mandi  
Shelly  
Javier  
Laverne".Split('\n');
        }

    }
}
