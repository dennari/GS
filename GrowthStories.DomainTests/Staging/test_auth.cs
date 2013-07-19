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


        private ILog Log = new LogTo4Net(typeof(AuthTest));


        [Test]
        public async void TestAuth()
        {

            await Sync();

            Assert.IsNotNullOrEmpty(Ctx.AccessToken);
            Assert.IsNotNullOrEmpty(Ctx.RefreshToken);
            Assert.Greater(Ctx.ExpiresIn, 0);
            //Assert.IsNull(auth.ExpiresIn);


        }

        Guid PlantId;
        GardenViewModel GVM;
        PlantViewModel PVM;

        [Test]
        public async void TestStagingSync()
        {

            GVM = Get<GardenViewModel>();
            PlantId = Guid.NewGuid();
            var PlantName = "Jore";

            GVM.NewPlantId = PlantId;
            GVM.NewPlantName = PlantName;
            GVM.AddPlantCommand.Execute(null);

            GVM.ShowDetailsCommand.Execute(GVM.Plants[0]);

            PVM = Get<PlantViewModel>();
            Assert.IsNotNull(PVM.Plant);
            Assert.AreEqual(PlantId, PVM.Plant.EntityId);
            //var UserId = Guid.NewGuid();
            //var PlantId = Guid.NewGuid();
            var PlantId2 = Guid.NewGuid();

            var Note = "EI NAIN!";
            var uri = new Uri("http://www.growthstories.com");

            PVM.AddCommentCommand.Execute(Note);
            PVM.AddPhotoCommand.Execute(uri);
            PVM.AddFertilizerCommand.Execute(null);
            PVM.AddWaterCommand.Execute(null);

            await Sync();

        }




        [Test]
        public async void TestAddRelationship()
        {

            Log.Info("TestAddRelationship");

            var fCmd = new CreateUser(Guid.NewGuid(), randomize("Bob"), randomize("swordfish"), randomize("bob") + "@wonderland.net");
            var Bob = Handler.Handle<User, CreateUser>(fCmd);

            await Sync();

            var relationshipCmd = new BecomeFollower(Ctx.Id, fCmd.EntityId);
            Handler.Handle<User, BecomeFollower>(relationshipCmd);

            await Sync();

        }





    }
}
