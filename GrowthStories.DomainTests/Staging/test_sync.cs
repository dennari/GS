using System;
using System.Linq;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using NUnit.Framework;
using Growthstories.Domain.Messaging;
//using Ninject;
using Growthstories.Core;
//using CommonDomain.Persistence;
using Growthstories.Sync;
//using Ninject.Parameters;
//using SimpleTesting;
//using Newtonsoft.Json;
using System.Threading.Tasks;
//using Newtonsoft.Json.Serialization;
//using System.Net.Http;
using System.Collections.Generic;
using Growthstories.Domain.Entities;
//using CommonDomain;
//using EventStore;

//using CommonDomain.Persistence.EventStore;
//using EventStore.Dispatcher;
using Growthstories.Domain;
using EventStore.Logging;
using Growthstories.UI;
//using System.Text;
using Growthstories.UI.ViewModel;
using Ninject;
using System.Net;
using System.Threading;
//using System.IO;
//using EventStore.Persistence;



namespace Growthstories.DomainTests
{
    public class StagingSyncTests : StagingTestBase
    {


        private GSLog Log = new GSLog(typeof(StagingSyncTests));

        private IAuthToken RemoteAuth;


        public void Setup()
        {

            GSLog.Logger.ViewerClearAll();

            this.Ctx = TestUtils.WaitForTask(App.Initialize());
            Assert.IsNotNull(Ctx);
            Assert.IsNotNullOrEmpty(Ctx.Username);


            WaitForTask(HttpClient.SendAsync(HttpClient.CreateClearDBRequest()));
            WaitForTask(App.PrepareAuthorizedUser());
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

            Setup();

            var originalRemoteEvents = await CreateRemoteData();

            var remoteUser = (UserCreated)originalRemoteEvents.OfType<UserCreated>().First();

            var listUsersResponse = await Transporter.ListUsersAsync(remoteUser.Username);

            var fetchedUser = listUsersResponse.Users[0];


            await App.HandleCommand(new CreateSyncStream(fetchedUser.AggregateId, PullStreamType.USER));

            var R2 = await App.Synchronize();
            SyncAssertions(R2);

            var remoteUserAggregate = (User)Repository.GetById(remoteUser.AggregateId);

            Assert.AreEqual(4, remoteUserAggregate.Version);
            Assert.AreEqual(remoteUser.Username, remoteUserAggregate.State.Username);
            Assert.AreEqual(1, remoteUserAggregate.State.Gardens.Count);
            Assert.AreEqual(originalRemoteEvents.OfType<GardenCreated>().Single().EntityId.Value, remoteUserAggregate.State.Garden.Id);
            Assert.AreEqual(originalRemoteEvents.OfType<PlantCreated>().Single().AggregateId, remoteUserAggregate.State.Garden.PlantIds[0]);

            var R3 = await App.Synchronize();
            //var R3 = R3s[0];
            SyncAssertions(R3);
            //Assert.IsTrue(R3.PushReq.IsEmpty);

        }


        protected T WaitForTask<T>(Task<T> task, int timeout = 9000)
        {
            if (Debugger.IsAttached)
                task.Wait();
            else
                task.Wait(timeout);
            Assert.IsTrue(task.IsCompleted, "Task timeout");
            return task.Result;
        }

        protected void WaitForTask(Task task, int timeout = 9000)
        {
            if (Debugger.IsAttached)
                task.Wait();
            else
                task.Wait(timeout);
            Assert.IsTrue(task.IsCompleted, "Task timeout");
        }

        protected T WaitForFirst<T>(IObservable<T> task, int timeout = 9000)
        {

            //task.Take(1).
            return WaitForTask(Task.Run(async () =>
            {

                return await task.Take(1);

            }), timeout);

        }

        [Test]
        public void RealTestAddUserViewModel()
        {
            TestAddUserViewModel();
        }


        public List<IEvent> TestAddUserViewModel()
        {
            Setup();


            var originalRemoteEvents = WaitForTask(CreateRemoteData(), 20000);

            var remoteUser = (UserCreated)originalRemoteEvents[0];

            var remotePlant = (PlantCreated)originalRemoteEvents[1];

            var remoteComment = (PlantActionCreated)originalRemoteEvents[3];

            var lvm = new SearchUsersViewModel(Get<ITransportEvents>(), App);

            IUserListResponse R = null;
            lvm.SearchResults.Subscribe(x =>
            {
                Log.Info("UserListResponse");
                R = x;
            });

            lvm.SearchCommand.Execute("Lauri");
            if (R == null)
                R = lvm.SearchResults.Take(1).Wait();
            Assert.IsNotNull(R);
            Assert.IsTrue(R.Users.Count > 0);


            var fvm = (FriendsViewModel)App.Resolver.GetService(typeof(FriendsViewModel));
            // this syncs automagically            
            lvm.UserSelectedCommand.Execute(R.Users[0]);
            var R3 = TestUtils.WaitForFirst(lvm.SyncResults);
            Assert.AreEqual(AllSyncResult.AllSynced, R3.Item1);


            //var R3 = WaitForTask(App.Synchronize());
            //SyncAssertions(R3);

            Assert.AreEqual(1, fvm.Friends.Count);
            var friend = fvm.Friends[0];
            Assert.AreEqual(remoteUser.AggregateId, friend.User.Id);
            Assert.AreEqual(remoteUser.Username, friend.User.Username);

            Assert.AreEqual(1, friend.Plants.Count);
            var plant = friend.Plants[0];

            Assert.AreEqual(remotePlant.AggregateId, plant.Id);
            Assert.AreEqual(remotePlant.Name, plant.Name);


            return originalRemoteEvents;

        }





        //[Test]
        //public async void RealTestReadModel()
        //{

        //    await TestSyncReadModel();

        //}

        //public  Task TestSyncReadModel()
        //{

        //    var originalRemoteEvents = TestAddUserViewModel();

        //    var remoteUser = (UserCreated)originalRemoteEvents[0];

        //    var remotePlant = (PlantCreated)originalRemoteEvents[3];

        //    var remoteComment = (PlantActionCreated)originalRemoteEvents[5];

        //    this.App = new TestAppViewModel(Kernel);
        //    this.Ctx = App.Context.CurrentUser;
        //    Assert.IsNotNull(Ctx);
        //    Assert.IsNotNullOrEmpty(Ctx.Username);

        //    var fvm = (FriendsViewModel)App.Resolver.GetService(typeof(FriendsViewModel));

        //    Assert.AreEqual(1, fvm.Friends.Count);
        //    var friend = fvm.Friends[0];
        //    Assert.AreEqual(remoteUser.AggregateId, friend.User.Id);
        //    Assert.AreEqual(remoteUser.Username, friend.User.Username);
        //}


        [Test]
        public async void RealTestSyncConflict()
        {

            await TestSyncConflict();

        }

        public async Task TestSyncConflict()
        {

            Setup();

            var plant = new CreatePlant(Guid.NewGuid(), "Jare", Ctx.GardenId, Ctx.Id);
            await App.HandleCommand(App.SetIds(plant));


            var R4 = await App.Synchronize();
            SyncAssertions(R4);


            var setNameCmd = App.SetIds(new SetName(plant.AggregateId, "Sepi"));
            await App.HandleCommand(setNameCmd);


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
        public void TestPhotoUpload()
        {
            Setup();
            // ARRANGE
            var u = App.User;
            Assert.IsNotNull(u);
            var plantId = Guid.NewGuid();
            var plantActionId = Guid.NewGuid();

            var photo = new Photo()
            {
                LocalFullPath = @"C:\Users\Ville\Documents\Visual Studio 2012\Projects\GrowthStories\GrowthStories.UI.WindowsPhone\Assets\Bg\plant_bg.jpg",
                LocalUri = "plant_bg.jpg",
                FileName = "plant_bg.jpg",
                Width = 500,
                Height = 500,
                Size = 500 * 500 * 3
            };

            //TestPushIncludesPublicEvents();


            TestUtils.WaitForTask(App.HandleCommand(new CreatePlant(plantId, "Jare", u.GardenId, u.Id)));
            TestUtils.WaitForTask(App.HandleCommand(new CreatePlantAction(plantActionId, u.Id, plantId, PlantActionType.PHOTOGRAPHED, "Just a photo")
            {
                Photo = photo
            }
            ));
            TestUtils.WaitForTask(App.HandleCommand(new SchedulePhotoUpload(photo, plantActionId)));


            //Assert.AreEqual(photo, AppState.PhotoUploads.Values.Single());

            Assert.IsNull(photo.BlobKey);
            var R = TestUtils.WaitForTask(App.Synchronize());
            //var R = Rs[0];

            Assert.IsNotNull(R.PhotoUploadRequests);
            Assert.IsNotNull(photo.BlobKey);


            //Assert.AreEqual(0, AppState.PhotoUploads.Count);


        }

        [Test]
        public void TestPhotoDownload()
        {
            Setup();
            var originalRemoteEvents = TestUtils.WaitForTask(CreateRemoteData());

        }


        [Test]
        public async void RealTestSyncPlantStream()
        {

            await TestSyncPlantStream();

        }

        public async Task TestSyncPlantStream()
        {

            Setup();

            var originalRemoteEvents = await CreateRemoteData();

            var remoteUser = (UserCreated)originalRemoteEvents.OfType<UserCreated>().First();

            var remotePlant = (PlantCreated)originalRemoteEvents.OfType<PlantCreated>().First();

            var remoteComment = (PlantActionCreated)originalRemoteEvents.OfType<PlantActionCreated>().Where(x => x.Type == PlantActionType.COMMENTED).First();

            var remotePhoto = (PlantActionCreated)originalRemoteEvents.OfType<PlantActionCreated>().Where(x => x.Type == PlantActionType.PHOTOGRAPHED).First();



            await App.HandleCommand(new CreateSyncStream(remotePlant.AggregateId, PullStreamType.PLANT, remoteUser.AggregateId));

            var R2 = await App.Synchronize();
            SyncAssertions(R2);

            var remotePlantAggregate = (Plant)Repository.GetById(remotePlant.AggregateId);

            Assert.AreEqual(2, remotePlantAggregate.Version);
            Assert.AreEqual(remotePlant.Name, remotePlantAggregate.State.Name);

            var remoteCommentAggregate = (PlantAction)Repository.GetById(remoteComment.AggregateId);

            Assert.AreEqual(1, remoteCommentAggregate.Version);
            Assert.AreEqual(remoteComment.Note, remoteCommentAggregate.State.Note);

            var remotePhotoAggregate = (PlantAction)Repository.GetById(remotePhoto.AggregateId);

            Assert.AreEqual(2, remotePhotoAggregate.Version);
            Assert.IsNotNull(remotePhotoAggregate.State.Photo.BlobKey);
            Assert.IsNotNull(remotePhotoAggregate.State.Photo.RemoteUri);



            var R3 = await App.Synchronize();
            SyncAssertions(R3);
            //Assert.IsNull(R3.PushReq);

        }



        [Test]
        public async void RealTestBecomeFollower()
        {

            await TestBecomeFollower();

        }

        PlantCreated RemotePlant = null;
        public async Task<BecomeFollower> TestBecomeFollower()
        {

            Setup();

            var originalRemoteEvents = await CreateRemoteData();

            var remoteUser = (UserCreated)originalRemoteEvents.OfType<UserCreated>().First();

            var remotePlant = (PlantCreated)originalRemoteEvents.OfType<PlantCreated>().First();

            var allR = await App.SyncAll();

            Assert.AreEqual(AllSyncResult.AllSynced, allR.Item1);

            Log.Warn("------------- TestBecomeFollower ------------------");

            var relationshipCmd = App.SetIds(new BecomeFollower(Ctx.Id, remoteUser.AggregateId));
            await App.HandleCommand(relationshipCmd);

            var R = await App.Synchronize();
            SyncAssertions(R);

            var stream = R.PushReq.Streams.First(x => x.AggregateId == Ctx.Id);

            Assert.AreEqual(1, stream.Count);
            Assert.IsInstanceOf<BecameFollower>(stream.First());

            Assert.IsTrue(App.SyncStreams.ContainsKey(remoteUser.AggregateId));
            Assert.IsTrue(App.SyncStreams.ContainsKey(remotePlant.AggregateId));
            User user = (User)(await App.GetById(Ctx.Id));
            Assert.IsTrue(user.State.Friends.ContainsKey(remoteUser.AggregateId));

            Log.Warn("------------- RESTART ------------------");

            var testUserName = "TestUser";
            var testUserEmail = "mail@jee.net";
            var testUserPw = "123456";

            var registerResponse = await App.Register(testUserName, testUserEmail, testUserPw);

            Assert.AreEqual(RegisterResponse.success, registerResponse);
            var signinResponse = await App.SignIn(testUserEmail, testUserPw);
            Assert.AreEqual(SignInResponse.success, signinResponse);


            Assert.IsTrue(App.SyncStreams.ContainsKey(remoteUser.AggregateId));
            Assert.IsTrue(App.SyncStreams.ContainsKey(remotePlant.AggregateId));

            user = (User)(await App.GetById(Ctx.Id));
            Assert.IsTrue(user.State.Friends.ContainsKey(remoteUser.AggregateId));

            RemotePlant = remotePlant;

            return relationshipCmd;

        }

        [Test]
        public async void RealTestUnFollow()
        {

            await TestUnFollow();

        }


        public async Task<IAggregateCommand> TestUnFollow()
        {

            var cmd = await TestBecomeFollower();

            var relationshipCmd = App.SetIds(new UnFollow(cmd.AggregateId, cmd.Target));
            await App.HandleCommand(relationshipCmd);

            var R = await App.Synchronize();
            SyncAssertions(R);

            var stream = R.PushReq.Streams.First(x => x.AggregateId == Ctx.Id);

            Assert.AreEqual(1, stream.Count);
            Assert.IsInstanceOf<UnFollowed>(stream.First());

            Assert.IsFalse(App.SyncStreams.ContainsKey(cmd.AggregateId));
            Assert.IsFalse(App.SyncStreams.ContainsKey(RemotePlant.AggregateId));
            User user = (User)(await App.GetById(Ctx.Id));
            Assert.IsFalse(user.State.Friends.ContainsKey(cmd.AggregateId));


            // start from zero and sync

            Log.Warn("------------- RESTART ------------------");

            var testUserName = "TestUser";
            var testUserEmail = "mail@jee.net";
            var testUserPw = "123456";

            var registerResponse = await App.Register(testUserName, testUserEmail, testUserPw);

            Assert.AreEqual(RegisterResponse.success, registerResponse);


            //Thread.Sleep(30000);

            //var registerResponse = TestUtils.WaitForTask(Transporter.RegisterAsync(testUserName, testUserEmail, testUserPw));
            //Assert.AreEqual(HttpStatusCode.OK, registerResponse.HttpStatus);


            var signinResponse = await App.SignIn(testUserEmail, testUserPw);

            Assert.AreEqual(SignInResponse.success, signinResponse);

            Assert.IsFalse(App.SyncStreams.ContainsKey(cmd.AggregateId));
            Assert.IsFalse(App.SyncStreams.ContainsKey(RemotePlant.AggregateId));
            user = (User)(await App.GetById(Ctx.Id));
            Assert.IsFalse(user.State.Friends.ContainsKey(cmd.AggregateId));



            return relationshipCmd;

        }






        protected async Task<List<IEvent>> CreateRemoteData()
        {
            // create remote data



            var users = ListOfNames().Select(x => TestUtils.CreateUserFromName(x.Trim())).ToArray();
            var remoteUser = users[0];

            var pushResp = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Streams = TestUtils.EventsToStreams(remoteUser.AggregateId, remoteUser),
                ClientDatabaseId = Guid.NewGuid(),
                Translator = Translator
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

            var photo = new Photo()
            {
                LocalFullPath = @"C:\Users\Ville\Documents\Visual Studio 2012\Projects\GrowthStories\GrowthStories.UI.WindowsPhone\Assets\Bg\plant_bg.jpg",
                LocalUri = "plant_bg.jpg",
                FileName = "plant_bg.jpg",
                Width = 500,
                Height = 500,
                Size = 500 * 500 * 3
            };


            var authResponse = await Transporter.RequestAuthAsync(remoteUser.Email, remoteUser.Password);


            var remotePhoto = new PlantActionCreated(
             App.SetIds(
            new CreatePlantAction(
                Guid.NewGuid(),
                remoteUser.AggregateId,
                remotePlant.AggregateId,
                PlantActionType.PHOTOGRAPHED,
                "Hello remote world") { Photo = photo },
            null, remoteUser.AggregateId))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var uploadRequest = new PhotoUploadRequest(photo, remotePhoto.AggregateId, Get<IJsonFactory>(), Transporter, Get<IPhotoHandler>());
            var uploadResponse = await uploadRequest.GetResponse();

            var remoteSetBlobKey = new BlobKeySet(App.SetIds(new SetBlobKey(remotePhoto.AggregateId, uploadResponse.BlobKey), null, remoteUser.AggregateId))
            {
                AggregateVersion = 2,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid(),
            };

            var remoteEvents = new List<IEvent>() { 
                remotePlant,
                remotePlantProperty,
                remoteComment,
                remotePhoto,
                remoteSetBlobKey,
                remoteGarden, 
                remoteAddGarden,                
                remoteAddPlant
            };






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

            ISyncPushResponse R = null;
            foreach (var e in events)
            {
                R = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
                {
                    Streams = new IStreamSegment[] { new StreamSegment(e.AggregateId, e) },
                    ClientDatabaseId = Guid.NewGuid(),
                    Translator = Translator
                });
                Assert.AreEqual(GSStatusCode.OK, R.StatusCode);

            }

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
