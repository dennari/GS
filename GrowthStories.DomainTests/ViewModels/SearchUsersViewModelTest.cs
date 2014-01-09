using Growthstories.Domain.Messaging;
using Growthstories.Domain;
using Growthstories.UI.ViewModel;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading;
using Growthstories.Domain.Entities;
using Growthstories.Sync;
using System.Collections.Generic;
using Ninject;
using System.Threading.Tasks;
using Growthstories.Core;


namespace Growthstories.DomainTests
{


    public class SearchUsersViewModelTest : ViewModelTestBase
    {

        protected PlantState State;

        [SetUp]
        public override void SetUp()
        {
            if (Kernel != null)
                Kernel.Dispose();
            Kernel = new StandardKernel(new SignInTestsSetup());
            App = new StagingAppViewModel(Kernel);
        }

        protected IPlantViewModel CreatePlantVM(IScheduleViewModel wateringSchedule = null, IScheduleViewModel fertilizingSchedule = null)
        {



            var name = "Sepi";
            var id = Guid.NewGuid();

            var plant = (Plant)Handler.Handle(new CreatePlant(id, name, U.GardenId, U.Id));
            this.State = plant.State;
            var vm = new PlantViewModel(State, wateringSchedule, fertilizingSchedule, App);
            Assert.AreEqual(id, vm.Id);
            Assert.AreEqual(name, vm.Name);
            Assert.AreEqual(id, State.Id);
            Assert.AreEqual(name, State.Name);
            return vm;


        }

        RemoteUser TestRemoteUser = new RemoteUser()
        {
            AggregateId = Guid.NewGuid(),
            Username = "TestRemoteUser",
            Email = "remoteuser@mail.net",
            Password = "123456",
            Garden = new RemoteGarden()
            {
                EntityId = Guid.NewGuid(),
                Plants = new List<RemotePlant>() {
                                        new RemotePlant()
                                        {
                                            AggregateId = Guid.NewGuid(),
                                            Name = "TestPlant"
                                        }
                                }
            }
        };

        [Test]
        public void TestListUsers()
        {
            // ARRANGE
            Assert.IsNull(App.User);
            var u = TestUtils.WaitForTask(App.Initialize());
            Assert.IsNotNull(App.User);

            var transporter = Kernel.Get<FakeHttpClient>();
            transporter.ListUsersFactory = username =>
            {
                return new UserListResponse()
                {
                    Users = new List<RemoteUser>()
                    {
                        TestRemoteUser
                    }
                };
            };
            var vm = new SearchUsersViewModel(transporter, App);

            // ACT
            vm.SearchCommand.Execute(TestRemoteUser.Username);

            // ASSERT
            var userList = TestUtils.WaitForFirst(vm.SearchResults);


            Assert.AreEqual(1, userList.Users.Count);
            Assert.AreEqual(TestRemoteUser.Username, userList.Users[0].Username);


        }

        [Test]
        public void TestFollowUser()
        {
            // ARRANGE
            Assert.IsNull(App.User);
            var u = TestUtils.WaitForTask(App.Initialize());
            Assert.IsNotNull(App.User);

            var transporter = Kernel.Get<FakeHttpClient>();

            var syncCounter = 0;
            transporter.PullResponseFactory = (r) =>
            {

                PullStream[] streams = new PullStream[] { };
                if (syncCounter == 0)
                {
                    streams = SyncEngineTests.CreatePullStream(TestRemoteUser.AggregateId, PullStreamType.USER,
                        new UserCreated(new CreateUser(TestRemoteUser.AggregateId, TestRemoteUser.Username, TestRemoteUser.Password, TestRemoteUser.Email))
                        {
                            AggregateVersion = 1,
                            Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 20),
                            MessageId = Guid.NewGuid()
                        },
                        new GardenCreated(new CreateGarden(TestRemoteUser.Garden.EntityId, TestRemoteUser.AggregateId))
                        {
                            AggregateVersion = 2,
                            Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 15),
                            MessageId = Guid.NewGuid()
                        },
                        new GardenAdded(new AddGarden(TestRemoteUser.AggregateId, TestRemoteUser.Garden.EntityId))
                        {
                            AggregateVersion = 3,
                            Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 15),
                            MessageId = Guid.NewGuid()
                        },
                        new PlantAdded(TestRemoteUser.AggregateId, TestRemoteUser.Garden.EntityId, TestRemoteUser.Garden.Plants[0].AggregateId)
                        {
                            AggregateVersion = 4,
                            Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 15),
                            MessageId = Guid.NewGuid()
                        });
                }
                else if (syncCounter == 1)
                {
                    var plant = TestRemoteUser.Garden.Plants[0];
                    streams = SyncEngineTests.CreatePullStream(plant.AggregateId, PullStreamType.PLANT,
                        new PlantCreated(new CreatePlant(plant.AggregateId, plant.Name, TestRemoteUser.Garden.EntityId, TestRemoteUser.AggregateId))
                        {
                            AggregateVersion = 1,
                            Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 20),
                            MessageId = Guid.NewGuid()
                        }
                       );
                }
                syncCounter++;
                return new HttpPullResponse()
                {
                    StatusCode = GSStatusCode.OK,
                    Projections = streams

                };
            };
            var vm = new SearchUsersViewModel(transporter, App);

            // ACT
            vm.UserSelectedCommand.Execute(TestRemoteUser);

            // ASSERT
            var result = TestUtils.WaitForFirst(vm.SyncResults);
            Assert.AreEqual(AllSyncResult.AllSynced, result.Item1);
            var user = (User)TestUtils.WaitForTask(App.GetById(App.User.Id));

            Assert.AreEqual(3, App.SyncStreams.Count);
            Assert.IsTrue(App.SyncStreams.ContainsKey(TestRemoteUser.AggregateId));
            Assert.IsTrue(App.SyncStreams.ContainsKey(TestRemoteUser.Garden.Plants[0].AggregateId));
            Assert.IsTrue(user.State.Friends.ContainsKey(TestRemoteUser.AggregateId));

        }

        [Test]
        public void TestUnFollowUser()
        {
            // ARRANGE
            TestFollowUser();


            TestUtils.WaitForTask(App.HandleCommand(new UnFollow(App.User.Id, TestRemoteUser.AggregateId)));

            var user = (User)TestUtils.WaitForTask(App.GetById(App.User.Id));

            Assert.IsFalse(user.State.Friends.ContainsKey(TestRemoteUser.AggregateId));

            Assert.IsFalse(App.SyncStreams.ContainsKey(TestRemoteUser.AggregateId));
            Assert.IsFalse(App.SyncStreams.ContainsKey(TestRemoteUser.Garden.Plants[0].AggregateId));


        }



    }
}
