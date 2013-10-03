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

        public async Task<IEntityCommand> TestCreateGarden()
        {

            await TestAuth();

            var garden = new CreateGarden(Guid.NewGuid(), Ctx.Id);

            Bus.SendCommand(garden);
            var addGarden = new AddGarden(Ctx.Id, garden.EntityId);

            Bus.SendCommand(addGarden);


            var R = await App.Synchronize();
            SyncAssertions(R);

            return garden;

        }


        public ISyncPushResponse SyncAssertions(SyncResult syncResult)
        {
            ISyncPushResponse lastResponse = null;

            Assert.IsInstanceOf<ISyncPushResponse>(syncResult.Communication.Last());
            try
            {
                lastResponse = (ISyncPushResponse)syncResult.Communication.Last();
            }
            catch (Exception)
            {

                Assert.Fail("last communication should be of type ISyncPushResponse");
            }

            Assert.AreEqual(GSStatusCode.OK, lastResponse.StatusCode);
            return lastResponse;
        }


        [Test]
        public async void RealTestCreateSchedule()
        {
            await TestCreateSchedule();
        }

        public async Task<IEntityCommand> TestCreateSchedule()
        {

            await TestAuth();
            var wateringSchedule = new CreateSchedule(Guid.NewGuid(), Ctx.Id, 24 * 2 * 3600);

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


        public async Task<IEntityCommand> TestCreatePlant()
        {

            var garden = await TestCreateGarden();

            var plant = new CreatePlant(Guid.NewGuid(), "Jore", Ctx.Id)
            {
                Profilepicture = new Photo(),
                Species = "Aloe Vera",
                Tags = new HashSet<string>() { "testtag", "testtag2" }
            };

            Bus.SendCommand(plant);

            var addPlant = new AddPlant(garden.EntityId, plant.EntityId, Ctx.Id, "Jore");

            Bus.SendCommand(addPlant);

            var wateringSchedule = new CreateSchedule(Guid.NewGuid(), Ctx.Id, 24 * 2 * 3600);

            Bus.SendCommand(wateringSchedule);

            var wateringScheduleSet = new SetWateringSchedule(plant.EntityId, wateringSchedule.EntityId);

            Bus.SendCommand(wateringScheduleSet);

            var R = await App.Synchronize();
            SyncAssertions(R);

            return plant;


        }

        [Test]
        public async void RealTestCreatePlantAction()
        {
            await TestCreatePlantAction();
        }


        public async Task<IEntityCommand> TestCreatePlantAction()
        {

            //await TestAuth();
            var plant = await TestCreatePlant();

            var comment = new CreatePlantAction(
                Guid.NewGuid(),
                Ctx.Id,
                plant.EntityId,
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


        public async Task<IEntityCommand> TestUpdatePlantAction()
        {

            //await TestAuth();
            var comment = (CreatePlantAction)(await TestCreatePlantAction());

            var prop = new SetPlantActionProperty(
                comment.EntityId,
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


        public async Task<IEntityCommand> TestAddRelationship()
        {

            var garden = await TestCreateGarden();

            // send some events directly
            var fCmd = new UserCreated(new CreateUser(
                Guid.NewGuid(),
                randomize("Bob"),
                randomize("swordfish"),
                randomize("bob") + "@wonderland.net"))
                {
                    EntityVersion = 1,
                    Created = DateTimeOffset.UtcNow,
                    EventId = Guid.NewGuid()
                };

            var plant = new PlantCreated(new CreatePlant(Guid.NewGuid(), "Jari", Ctx.Id)
            {
                Profilepicture = new Photo(),
                Species = "Aloe Vera",
                Tags = new HashSet<string>() { "testtag", "testtag2" }
            })
            {
                EntityVersion = 1,
                Created = DateTimeOffset.UtcNow,
                EventId = Guid.NewGuid()
            };


            var addPlant = new PlantAdded(new AddPlant(garden.EntityId, plant.EntityId, Ctx.Id, "Jare"))
            {
                EntityVersion = 4,
                Created = DateTimeOffset.UtcNow,
                EventId = Guid.NewGuid()
            };



            var pushResp = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Events = Translator.Out(new IEvent[] { plant, addPlant, fCmd }).ToArray(),
                ClientDatabaseId = Guid.NewGuid()
            });

            Assert.AreEqual(GSStatusCode.OK, pushResp.StatusCode);


            Log.Info("TestAddRelationship");



            var relationshipCmd = new BecomeFollower(Ctx.Id, fCmd.EntityId, Guid.NewGuid());
            Bus.SendCommand(relationshipCmd);

            var friendshipCmd = new RequestFriendship(Ctx.Id, relationshipCmd.RelationshipId);
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

            var addPlant2 = new AddPlant(garden.EntityId, plant2.EntityId, Ctx.Id, "Jare");
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
            Assert.AreEqual(remoteUser.EntityId, fetchedUser.EntityId);



            //var relationshipCmd = new BecomeFollower(Ctx.Id, remoteUser.EntityId, Guid.NewGuid());
            //Bus.SendCommand(relationshipCmd);

            var pullRequest = RequestFactory.CreatePullRequest(
                    new Guid[] { remoteUser.EntityId }.Select(x => new SyncEventStream(x, EventStore) { Type = "USER" }));

            var userStream = pullRequest.Streams.First() as SyncEventStream;

            var pullResponse = await Transporter.PullAsync(pullRequest);


            Assert.AreEqual(GSStatusCode.OK, pullResponse.StatusCode);
            var remoteEvents = pullResponse.Events.ToArray();
            Assert.AreEqual(1, remoteEvents.Length);
            Assert.AreEqual(remoteUser.EntityId, remoteEvents[0].Key);

            foreach (var remoteEvent in remoteEvents[0])
                userStream.AddRemote(remoteEvent);

            userStream.CommitRemoteChanges(Guid.NewGuid());


            var remoteUserAggregate = (User)Repository.GetById(remoteUser.EntityId);

            Assert.AreEqual(4, remoteUserAggregate.Version);
            Assert.AreEqual(remoteUser.Username, remoteUserAggregate.State.Username);
            Assert.AreEqual(1, remoteUserAggregate.State.Gardens.Count);
            Assert.AreEqual(originalRemoteEvents[1].EntityId, remoteUserAggregate.State.GardenId);
            Assert.AreEqual(originalRemoteEvents[3].EntityId, remoteUserAggregate.State.Gardens[remoteUserAggregate.State.GardenId].PlantIds[0]);

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

            var file = File.Open(@"C:\Users\Ville\Documents\Visual Studio 2012\Projects\GrowthStories\GrowthStories.DomainTests\plant_bg.jpg", FileMode.Open);

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
                EntityVersion = 1,
                Created = DateTimeOffset.UtcNow,
                EventId = Guid.NewGuid()
            };

            var pushResp = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Events = new IEventDTO[] { Translator.Out(remoteUser) },
                ClientDatabaseId = Guid.NewGuid()
            });
            Assert.AreEqual(GSStatusCode.OK, pushResp.StatusCode);

            var remoteGarden = new GardenCreated(new CreateGarden(Guid.NewGuid(), remoteUser.EntityId))
            {
                EntityVersion = 2,
                Created = DateTimeOffset.UtcNow,
                EventId = Guid.NewGuid()
            };

            var remoteAddGarden = new GardenAdded(new AddGarden(remoteUser.EntityId, remoteGarden.EntityId))
            {
                EntityVersion = 3,
                Created = DateTimeOffset.UtcNow,
                EventId = Guid.NewGuid()
            };

            var remotePlant = new PlantCreated(new CreatePlant(Guid.NewGuid(), "RemoteJare", remoteUser.EntityId))
            {
                EntityVersion = 1,
                Created = DateTimeOffset.UtcNow,
                EventId = Guid.NewGuid()
            };


            var remoteAddPlant = new PlantAdded(new AddPlant(remoteGarden.EntityId, remotePlant.EntityId, remoteUser.EntityId, "RemoteJare"))
            {
                EntityVersion = 4,
                Created = DateTimeOffset.UtcNow,
                EventId = Guid.NewGuid()
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
