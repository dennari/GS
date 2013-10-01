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

            await ClearRemoteDB();
            var R = await App.Synchronize();

            Assert.IsNotNullOrEmpty(Ctx.AccessToken);
            Assert.IsNotNullOrEmpty(Ctx.RefreshToken);
            Assert.Greater(Ctx.ExpiresIn, 0);
            //Assert.IsNull(auth.ExpiresIn);
            SyncAssertions(R);


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

            Assert.AreEqual(200, lastResponse.StatusCode);
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



            var pushResp = await Transporter.PushAsync(new HttpPushRequest()
            {
                Events = Translator.Out(new IEvent[] { plant, addPlant, fCmd }).ToArray(),
                ClientDatabaseId = Guid.NewGuid()
            });

            Assert.AreEqual(200, pushResp.StatusCode);


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





    }
}
