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
        public void TestAssignUser()
        {

            this.Ctx = TestUtils.WaitForTask(App.Initialize());
            Assert.IsNotNull(Ctx);
            Assert.IsNotNullOrEmpty(Ctx.Username);


        }

        [Test]
        public async void RealTestAuth()
        {

            await TestAuth();


        }

        public async Task TestAuth()
        {


            TestAssignUser();
            await HttpClient.SendAsync(HttpClient.CreateClearDBRequest());
            //await Get<ISynchronizerService>().CreateUserAsync(Ctx.Id);


            await App.Context.AuthorizeUser();
            //Ctx = App.Context.CurrentUser;

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

            await App.HandleCommand(garden);
            //var addGarden = App.SetIds(new AddGarden(Ctx.Id, garden.EntityId.Value));

            //Bus.SendCommand(addGarden);


            var R = await App.Synchronize();
            SyncAssertions(R);

            return garden;

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

            await App.HandleCommand(wateringSchedule);


            var R = await App.Synchronize();
            //var R = Rs[0];
            SyncAssertions(R, true);

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

            var plant = App.SetIds(new CreatePlant(Guid.NewGuid(), "Jore", garden.EntityId.Value, Ctx.Id)
            {
                Profilepicture = new Photo(),
                Species = "Aloe Vera",
                Tags = new HashSet<string>() { "testtag", "testtag2" },
            });

            await App.HandleCommand(plant);

            var addPlant = App.SetIds(new AddPlant(garden.EntityId.Value, plant.AggregateId, Ctx.Id, "Jore"));

            await App.HandleCommand(addPlant);

            //var wateringSchedule = App.SetIds(new CreateSchedule(Guid.NewGuid(), 24 * 2 * 3600));

            //Bus.SendCommand(wateringSchedule);

            //var wateringScheduleSet = new SetWateringSchedule(plant.AggregateId, wateringSchedule.AggregateId);

            //Bus.SendCommand(wateringScheduleSet);

            var R = await App.Synchronize();
            //var R = Rs[0];
            SyncAssertions(R, true);

            var R2 = await App.Synchronize();
            //var R = Rs[0];
            SyncAssertions(R2, true);

            var R3 = await App.Synchronize();
            //var R = Rs[0];
            Assert.IsTrue(R3.PushReq.IsEmpty);



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

            var comment = App.SetIds(new CreatePlantAction(
                Guid.NewGuid(),
                Ctx.Id,
                plant.AggregateId,
                PlantActionType.COMMENTED,
                "new note"));

            await App.HandleCommand(comment);


            var R = await App.Synchronize();
            //var R = Rs[0];
            SyncAssertions(R, true);

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

            var prop = App.SetIds(new SetPlantActionProperty(
                comment.AggregateId
                )
                {
                    Note = "Updated note"
                });

            await App.HandleCommand(prop);


            var R = await App.Synchronize();
            //var R = Rs[0];
            SyncAssertions(R, true);

            return comment;
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
            var R = await T.Upload(uploadUriResponse.PhotoUri, file);

            Assert.IsTrue(R.Item1.IsSuccessStatusCode);

            Log.Info(R.Item2);


        }







    }
}
