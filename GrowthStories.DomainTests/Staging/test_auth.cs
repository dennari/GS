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
using Growthstories.UI;
using System.Text;
using Growthstories.UI.ViewModel;
using System.IO;
using EventStore.Persistence;



namespace Growthstories.DomainTests
{
    public class AuthTest : StagingTestBase
    {


        private ILog Log = new LogToNLog(typeof(AuthTest));


        [Test]
        public async void RealTestAuth()
        {

            await TestAuth();


        }

        public async Task TestAuth()
        {

            await HttpClient.SendAsync(HttpClient.CreateClearDBRequest());
            await Get<IUserService>().AuthorizeUser();


            Assert.IsNotNullOrEmpty(Ctx.AccessToken);
            Assert.IsNotNullOrEmpty(Ctx.RefreshToken);
            Assert.Greater(Ctx.ExpiresIn, 0);
            //Assert.IsNull(auth.ExpiresIn);
            //SyncAssertions(R);


        }

        [Test]
        public async void RealTestCreateGarden()
        {

            await TestCreateGarden();


        }

        public async Task<IAggregateCommand> TestCreateGarden()
        {

            await TestAuth();

            var garden = App.SetIds(new CreateGarden(Guid.NewGuid(), Ctx.Id));

            Bus.SendCommand(garden);
            //var addGarden = App.SetIds(new AddGarden(Ctx.Id, garden.EntityId.Value));

            //Bus.SendCommand(addGarden);


            var R = await App.Synchronize();
            SyncAssertions(R);

            return garden;

        }


        public ISyncPushResponse SyncAssertions(SyncResult syncResult)
        {
            // if everything goes smoothly, we should have a single pull and a single push
            Assert.AreEqual(1, syncResult.Pushes.Count);
            Assert.AreEqual(1, syncResult.Pulls.Count);
            Assert.IsNotNull(syncResult.Pulls[0].Item2);
            Assert.AreEqual(GSStatusCode.OK, syncResult.Pulls[0].Item2.StatusCode);

            if (syncResult.Pushes[0].Item2 != null)
                Assert.AreEqual(GSStatusCode.OK, syncResult.Pushes[0].Item2.StatusCode);


            return syncResult.Pushes[0].Item2;
        }


        [Test]
        public async void RealTestCreateSchedule()
        {
            await TestCreateSchedule();
        }

        public async Task<IAggregateCommand> TestCreateSchedule()
        {

            await TestAuth();
            var wateringSchedule = App.SetIds(new CreateSchedule(Guid.NewGuid(), 24 * 2 * 3600));

            Bus.SendCommand(wateringSchedule);


            var R = await App.Synchronize();
            SyncAssertions(R);

            return wateringSchedule;
        }


        [Test]
        public async void RealTestCreatePlant()
        {
            await TestCreatePlant();
        }


        public async Task<IAggregateCommand> TestCreatePlant()
        {

            var garden = await TestCreateGarden();

            var plant = App.SetIds(new CreatePlant(Guid.NewGuid(), "Jore", garden.EntityId.Value)
            {
                Profilepicture = new Photo(),
                Species = "Aloe Vera",
                Tags = new HashSet<string>() { "testtag", "testtag2" },
            });

            Bus.SendCommand(plant);

            var addPlant = App.SetIds(new AddPlant(garden.EntityId.Value, plant.AggregateId, Ctx.Id, "Jore"));

            Bus.SendCommand(addPlant);

            //var wateringSchedule = App.SetIds(new CreateSchedule(Guid.NewGuid(), 24 * 2 * 3600));

            //Bus.SendCommand(wateringSchedule);

            //var wateringScheduleSet = new SetWateringSchedule(plant.AggregateId, wateringSchedule.AggregateId);

            //Bus.SendCommand(wateringScheduleSet);

            var R = await App.Synchronize();
            SyncAssertions(R);

            return plant;


        }

        [Test]
        public async void RealTestCreatePlantAction()
        {
            await TestCreatePlantAction();
        }


        public async Task<IAggregateCommand> TestCreatePlantAction()
        {

            //await TestAuth();
            var plant = await TestCreatePlant();

            var comment = new CreatePlantAction(
                Guid.NewGuid(),
                Ctx.Id,
                plant.AggregateId,
                PlantActionType.COMMENTED,
                "new note");

            Bus.SendCommand(comment);


            var R = await App.Synchronize();
            SyncAssertions(R);

            return comment;
        }

        [Test]
        public async void RealTestUpdatePlantAction()
        {
            await TestUpdatePlantAction();
        }


        public async Task<IAggregateCommand> TestUpdatePlantAction()
        {

            //await TestAuth();
            var comment = (CreatePlantAction)(await TestCreatePlantAction());

            var prop = new SetPlantActionProperty(
                comment.AggregateId,
                comment.PlantId
                )
                {
                    Note = "Updated note"
                };

            Bus.SendCommand(prop);


            var R = await App.Synchronize();
            SyncAssertions(R);

            return comment;
        }


        [Test]
        public async void RealTestAddRelationship()
        {

            await TestAddRelationship();

        }


        public async Task<IAggregateCommand> TestAddRelationship()
        {

            var garden = await TestCreateGarden();

            // send some events directly
            var fCmd = new UserCreated(new CreateUser(
                Guid.NewGuid(),
                randomize("Bob"),
                randomize("swordfish"),
                randomize("bob") + "@wonderland.net"))
                {
                    AggregateVersion = 1,
                    Created = DateTimeOffset.UtcNow,
                    MessageId = Guid.NewGuid()
                };

            var plant = new PlantCreated(new CreatePlant(Guid.NewGuid(), "Jari", Ctx.Id)
            {
                Profilepicture = new Photo(),
                Species = "Aloe Vera",
                Tags = new HashSet<string>() { "testtag", "testtag2" }
            })
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };


            var addPlant = new PlantAdded(new AddPlant(garden.EntityId.Value, plant.AggregateId, Ctx.Id, "Jare"))
            {
                AggregateVersion = 4,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };



            var pushResp = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Events = Translator.Out(new IEvent[] { plant, addPlant, fCmd }).ToArray(),
                ClientDatabaseId = Guid.NewGuid()
            });

            Assert.AreEqual(GSStatusCode.OK, pushResp.StatusCode);


            Log.Info("TestAddRelationship");



            var relationshipCmd = new BecomeFollower(Ctx.Id, fCmd.AggregateId, Guid.NewGuid());
            Bus.SendCommand(relationshipCmd);

            var friendshipCmd = new RequestFriendship(Ctx.Id, relationshipCmd.EntityId.Value);
            Bus.SendCommand(friendshipCmd);


            var R = await App.Synchronize();
            SyncAssertions(R);


            var plant2 = new CreatePlant(Guid.NewGuid(), "Jari", Ctx.Id)
            {
                Profilepicture = new Photo(),
                Species = "Aloe Vera",
                Tags = new HashSet<string>() { "testtag", "testtag2" }
            };
            Bus.SendCommand(plant2);

            var addPlant2 = new AddPlant(garden.EntityId.Value, plant2.AggregateId, Ctx.Id, "Jare");
            Bus.SendCommand(addPlant2);


            var R2 = await App.Synchronize();
            SyncAssertions(R2);


            return relationshipCmd;

        }

        [Test]
        public async void RealTestPullRemoteUser()
        {

            await TestPullRemoteUser();

        }

        public async Task TestPullRemoteUser()
        {

            var garden = await TestCreateGarden();

            var originalRemoteEvents = await CreateRemoteData();

            var remoteUser = (UserCreated)originalRemoteEvents[0];

            var listUsersResponse = await Transporter.ListUsersAsync(remoteUser.Username);

            Assert.AreEqual(GSStatusCode.OK, listUsersResponse.StatusCode);
            Assert.AreEqual(1, listUsersResponse.Users.Count);

            var fetchedUser = listUsersResponse.Users[0];
            Assert.AreEqual(remoteUser.Username, fetchedUser.Username);
            Assert.AreEqual(remoteUser.AggregateId, fetchedUser.EntityId);



            //var relationshipCmd = new BecomeFollower(Ctx.Id, remoteUser.EntityId, Guid.NewGuid());
            //Bus.SendCommand(relationshipCmd);

            var pullRequest = RequestFactory.CreatePullRequest(
                new Guid[] { remoteUser.AggregateId }.Select(x => new SyncEventStream(x, EventStore, Get<IPersistSyncStreams>()) { Type = SyncStreamType.USER }));
            //;

            //var userStream = pullRequest.Streams.First() as SyncEventStream;

            var pullResponse = await Transporter.PullAsync(pullRequest);


            Assert.AreEqual(GSStatusCode.OK, pullResponse.StatusCode);
            var remoteStreams = pullResponse.Streams.ToArray();
            Assert.AreEqual(1, remoteStreams.Length);
            Assert.AreEqual(remoteUser.AggregateId, remoteStreams[0].StreamId);


            remoteStreams[0].CommitRemoteChanges(Guid.NewGuid());


            var remoteUserAggregate = (User)Repository.GetById(remoteUser.AggregateId);

            Assert.AreEqual(4, remoteUserAggregate.Version);
            Assert.AreEqual(remoteUser.Username, remoteUserAggregate.State.Username);
            Assert.AreEqual(1, remoteUserAggregate.State.Gardens.Count);
            Assert.AreEqual(originalRemoteEvents[1].EntityId.Value, remoteUserAggregate.State.GardenId);
            Assert.AreEqual(originalRemoteEvents[3].AggregateId, remoteUserAggregate.State.Gardens[remoteUserAggregate.State.GardenId].PlantIds[0]);

        }


        [Test]
        public async void RealTestSync()
        {

            await TestSync();

        }

        public async Task TestSync()
        {

            var garden = await TestCreateGarden();

            var originalRemoteEvents = await CreateRemoteData();

            var remoteUser = (UserCreated)originalRemoteEvents[0];

            var listUsersResponse = await Transporter.ListUsersAsync(remoteUser.Username);

            var fetchedUser = listUsersResponse.Users[0];

            var stream = new SyncEventStream(fetchedUser.EntityId, EventStore, Get<IPersistSyncStreams>())
            {
                Type = SyncStreamType.USER
            };
            stream.AddRemote(
            new UserCreated(App.SetIds(new CreateUser(
                fetchedUser.EntityId,
                fetchedUser.Username,
                randomize("swordfish"),
                randomize("bob") + "@wonderland.net"), null, fetchedUser.EntityId))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            });
            stream.CommitRemoteChanges(Guid.NewGuid());

            var R2 = await App.Synchronize();
            SyncAssertions(R2);

            var remoteUserAggregate = (User)Repository.GetById(remoteUser.AggregateId);

            Assert.AreEqual(4, remoteUserAggregate.Version);
            Assert.AreEqual(remoteUser.Username, remoteUserAggregate.State.Username);
            Assert.AreEqual(1, remoteUserAggregate.State.Gardens.Count);
            Assert.AreEqual(originalRemoteEvents[1].EntityId.Value, remoteUserAggregate.State.GardenId);
            Assert.AreEqual(originalRemoteEvents[3].AggregateId, remoteUserAggregate.State.Gardens[remoteUserAggregate.State.GardenId].PlantIds[0]);

            var R3 = await App.Synchronize();
            SyncAssertions(R3);
            Assert.IsNull(R3.Pushes[0].Item2);

        }



        [Test]
        public async void RealTestPhoto()
        {

            await TestPhoto();

        }

        public async Task TestPhoto()
        {
            var plant = (CreatePlant)(await TestCreatePlant());

            var uploadUriResponse = await Transporter.RequestPhotoUploadUri();

            Assert.AreEqual(GSStatusCode.OK, uploadUriResponse.StatusCode);


            var T = Transporter as SyncHttpClient;

            var file = File.Open(@"C:\Users\Ville\Documents\Visual Studio 2012\Projects\GrowthStories\GrowthStories.UI.WindowsPhone\Assets\Bg\plant_bg.jpg", FileMode.Open);

            T.AuthToken = null;
            var R = await T.Upload(uploadUriResponse.UploadUri, file);

            Assert.IsTrue(R.IsSuccessStatusCode);

            var blobkey = await R.Content.ReadAsStringAsync();

            Log.Info(blobkey);


        }


        protected async Task<List<IEvent>> CreateRemoteData()
        {
            // create remote data
            var remoteUser = new UserCreated(new CreateUser(
                Guid.NewGuid(),
                randomize("Bob"),
                randomize("swordfish"),
                randomize("bob") + "@wonderland.net"))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var pushResp = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Events = new IEventDTO[] { Translator.Out(remoteUser) },
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

            var remotePlant = new PlantCreated(App.SetIds(new CreatePlant(Guid.NewGuid(), "RemoteJare", remoteGarden.EntityId.Value), null, remoteUser.AggregateId))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };


            var remoteAddPlant = new PlantAdded(App.SetIds(new AddPlant(remoteGarden.EntityId.Value, remotePlant.AggregateId, remoteUser.AggregateId, "RemoteJare"), null, remoteUser.AggregateId))
            {
                AggregateVersion = 4,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var remoteEvents = new List<IEvent>() { 
                remoteGarden, 
                remoteAddGarden, 
                remotePlant, 
                remoteAddPlant 
            };



            var currentAuth = HttpClient.AuthToken;

            var authResponse = await Transporter.RequestAuthAsync(remoteUser.Username, remoteUser.Password);
            Assert.AreEqual(authResponse.StatusCode, GSStatusCode.OK);
            HttpClient.AuthToken = authResponse.AuthToken;

            var pushResp2 = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Events = Translator.Out(remoteEvents).ToArray(),
                ClientDatabaseId = Guid.NewGuid()
            });

            Assert.AreEqual(GSStatusCode.OK, pushResp2.StatusCode);

            HttpClient.AuthToken = currentAuth;

            remoteEvents.Insert(0, remoteUser);

            return remoteEvents;
        }





    }
}
