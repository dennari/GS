using System;
using System.Linq;
using System.Collections.Generic;

using Growthstories.Sync;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.DomainTests.Sync;
using Growthstories.UI.ViewModel;

using NUnit.Framework;
using Ninject;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Growthstories.DomainTests
{



    public class SyncEngineTests
    {

        IKernel Kernel;
        IGSAppViewModel App;
        IGSRepository Repository;
        FakeHttpClient Transporter;

        [SetUp]
        public void SetUp()
        {
            //if (kernel != null)
            //    kernel.Dispose();
            Kernel = new StandardKernel(new SyncEngineTestsSetup());

            App = new Growthstories.DomainTests.StagingTestBase.TestAppViewModel(Kernel);
            Transporter = Kernel.Get<FakeHttpClient>();
            Repository = Kernel.Get<IGSRepository>();
        }

        public string toJSON(object o) { return Kernel.Get<IJsonFactory>().Serialize(o); }
        public T fromJSON<T>(string s) { return Kernel.Get<IJsonFactory>().Deserialize<T>(s); }

        protected ITranslateEvents Translator
        {
            get
            {
                return Kernel.Get<ITranslateEvents>();
            }
        }

        [Test]
        public void TestPushIncludesPublicEvents()
        {
            var u = App.Context.CurrentUser;
            App.Bus.SendCommand(new CreateUser(u.Id, u.Username, u.Password, u.Email));
            App.Bus.SendCommand(new AssignAppUser(u.Id, u.Username, u.Password, u.Email));


            var R = WaitForTask(App.Synchronize());

            Assert.IsFalse(R.PushReq.IsEmpty);

            Assert.AreEqual(u.Id, R.PushReq.Streams.Single().Single().AggregateId);

        }

        [Test]
        public void TestCreatedUserIsInPullRequest()
        {
            var u = App.Context.CurrentUser;
            App.Bus.SendCommand(new CreateUser(u.Id, u.Username, u.Password, u.Email));
            App.Bus.SendCommand(new AssignAppUser(u.Id, u.Username, u.Password, u.Email));


            var R = WaitForTask(App.Synchronize());

            Assert.IsFalse(R.PullReq.IsEmpty);
            Assert.AreEqual(u.Id, R.PullReq.Streams.Single().StreamId);
        }




        public static IStreamSegment[] CreateStreams(params IMessage[] msgs)
        {
            return msgs.GroupBy(x => x.AggregateId).Select(x => new StreamSegment(x)).ToArray();
        }

        [Test]
        public void TestPullCanCreateStream()
        {

            var remoteUser = StagingSyncTests.CreateUserFromName("Jorma");

            Transporter.PullResponseFactory = (r) => new HttpPullResponse()
            {
                StatusCode = GSStatusCode.OK,
                Streams = CreateStreams(remoteUser)
            };

            App.Bus.SendCommand(new CreateSyncStream(remoteUser.AggregateId, PullStreamType.USER));


            var R = WaitForTask(App.Synchronize());

            var RemoteUser = Repository.GetById(remoteUser.AggregateId);
            Assert.IsInstanceOf<User>(RemoteUser);
            Assert.AreEqual(1, RemoteUser.Version);

        }



        [Test]
        public void TestCommutativeConflict()
        {

            // ARRANGE
            var u = App.Context.CurrentUser;
            Guid localTarget = Guid.NewGuid();
            Guid remoteTarget = Guid.NewGuid();

            TestPushIncludesPublicEvents();


            Transporter.PullResponseFactory = (r) => new HttpPullResponse()
            {
                StatusCode = GSStatusCode.OK,
                Streams = CreateStreams(new BecameFollower(new BecomeFollower(u.Id, remoteTarget))
                {
                    AggregateVersion = 2,
                    Created = DateTimeOffset.UtcNow,
                    MessageId = Guid.NewGuid()
                })
            };

            App.Bus.SendCommand(new BecomeFollower(u.Id, localTarget));

            var R = WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);

            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(3, translated[0].AggregateVersion);


            var User = Repository.GetById(u.Id);
            Assert.IsInstanceOf<User>(User);
            Assert.AreEqual(3, User.Version);

        }


        [Test]
        public void TestNonCommutativeConflict()
        {

            // ARRANGE
            var u = App.Context.CurrentUser;
            Guid localTarget = Guid.NewGuid();
            Guid remoteTarget = Guid.NewGuid();
            var remoteName = "JeppeRemote";
            var localName = "JeppeLocal";

            TestPushIncludesPublicEvents();


            Transporter.PullResponseFactory = (r) => new HttpPullResponse()
            {
                StatusCode = GSStatusCode.OK,
                Streams = CreateStreams(new UsernameSet(new SetUsername(u.Id, remoteName))
                {
                    AggregateVersion = 2,
                    Created = DateTimeOffset.UtcNow,
                    MessageId = Guid.NewGuid()
                })
            };

            App.Bus.SendCommand(new SetUsername(u.Id, localName));

            var R = WaitForTask(App.Synchronize());

            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);
            Assert.IsInstanceOf<INullEvent>(pushStream.Single());

            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(3, translated.Single().AggregateVersion);


            var pullStream = R.PullResp.Streams.Single();
            Assert.AreEqual(1, pullStream.Count);
            Assert.IsInstanceOf<UsernameSet>(pullStream.Single());

            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(3, User.Version);

            Assert.AreEqual(remoteName, User.State.Username);



        }



        [Test]
        public void TestNonCommutativeConflict2()
        {

            // ARRANGE
            var u = App.Context.CurrentUser;
            Guid localTarget = Guid.NewGuid();
            Guid remoteTarget = Guid.NewGuid();
            var remoteName = "JeppeRemote";
            var localName = "JeppeLocal";

            TestPushIncludesPublicEvents();


            Transporter.PullResponseFactory = (r) => new HttpPullResponse()
            {
                StatusCode = GSStatusCode.OK,
                Streams = CreateStreams(new UsernameSet(new SetUsername(u.Id, remoteName))
                {
                    AggregateVersion = 2,
                    Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 20),
                    MessageId = Guid.NewGuid()
                })
            };

            App.Bus.SendCommand(new SetUsername(u.Id, localName));

            var R = WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);
            Assert.IsInstanceOf<UsernameSet>(pushStream.Single());


            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(3, translated.Single().AggregateVersion);

            var pullStream = R.PullResp.Streams.Single();
            Assert.AreEqual(1, pullStream.Count);
            Assert.IsInstanceOf<INullEvent>(pullStream.Single());


            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(3, User.Version);

            Assert.AreEqual(localName, User.State.Username);



        }



        [Test]
        public void TestNonCommutativeConflictWithMultipleConflicts()
        {

            // ARRANGE
            var u = App.Context.CurrentUser;
            Guid localTarget = Guid.NewGuid();
            Guid remoteTarget = Guid.NewGuid();
            var remoteName = "JeppeRemote";
            var newerRemoteName = "JeppeRemoteNewer";
            var localName = "JeppeLocal";

            TestPushIncludesPublicEvents();


            Transporter.PullResponseFactory = (r) => new HttpPullResponse()
            {
                StatusCode = GSStatusCode.OK,
                Streams = CreateStreams(new UsernameSet(new SetUsername(u.Id, remoteName))
                {
                    AggregateVersion = 2,
                    Created = DateTimeOffset.UtcNow,
                    MessageId = Guid.NewGuid()
                },
                new BecameFollower(new BecomeFollower(u.Id, remoteTarget))
                {
                    AggregateVersion = 3,
                    Created = DateTimeOffset.UtcNow,
                    MessageId = Guid.NewGuid()
                },
                new UsernameSet(new SetUsername(u.Id, newerRemoteName))
                {
                    AggregateVersion = 4,
                    Created = DateTimeOffset.UtcNow,
                    MessageId = Guid.NewGuid()
                }
                )

            };

            App.Bus.SendCommand(new SetUsername(u.Id, localName));

            var R = WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);
            Assert.IsInstanceOf<INullEvent>(pushStream.Single());


            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(5, translated.Single().AggregateVersion);

            //var pullStream = R.PullResp.Streams.Single();
            //Assert.AreEqual(1, pullStream.Count);
            //Assert.IsInstanceOf<INullEvent>(pullStream.Single());


            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(5, User.Version);

            Assert.AreEqual(newerRemoteName, User.State.Username);



        }


        [Test]
        public void TestNonCommutativeConflictWithMultipleConflicts2()
        {

            // ARRANGE
            var u = App.Context.CurrentUser;
            Guid localTarget = Guid.NewGuid();
            Guid remoteTarget = Guid.NewGuid();
            var remoteName = "JeppeRemote";
            var newerRemoteName = "JeppeRemoteNewer";
            var localName = "JeppeLocal";

            TestPushIncludesPublicEvents();


            Transporter.PullResponseFactory = (r) => new HttpPullResponse()
            {
                StatusCode = GSStatusCode.OK,
                Streams = CreateStreams(new UsernameSet(new SetUsername(u.Id, remoteName))
                {
                    AggregateVersion = 2,
                    Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 20),
                    MessageId = Guid.NewGuid()
                },
                new BecameFollower(new BecomeFollower(u.Id, remoteTarget))
                {
                    AggregateVersion = 3,
                    Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 15),
                    MessageId = Guid.NewGuid()
                },
                new UsernameSet(new SetUsername(u.Id, newerRemoteName))
                {
                    AggregateVersion = 4,
                    Created = DateTimeOffset.UtcNow,
                    MessageId = Guid.NewGuid()
                }
                )

            };

            App.Bus.SendCommand(new SetUsername(u.Id, localName));

            var R = WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);
            Assert.IsInstanceOf<INullEvent>(pushStream.Single());


            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(5, translated.Single().AggregateVersion);

            //var pullStream = R.PullResp.Streams.Single();
            //Assert.AreEqual(1, pullStream.Count);
            //Assert.IsInstanceOf<INullEvent>(pullStream.Single());


            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(5, User.Version);

            Assert.AreEqual(newerRemoteName, User.State.Username);



        }



        [Test]
        public void TestNonCommutativeConflictWithMultipleConflicts3()
        {

            // ARRANGE
            var u = App.Context.CurrentUser;
            Guid localTarget = Guid.NewGuid();
            Guid remoteTarget = Guid.NewGuid();
            var remoteName = "JeppeRemote";
            var newerRemoteName = "JeppeRemoteNewer";
            var localName = "JeppeLocal";
            var newerLocalName = "JeppeLocalNewer";

            TestPushIncludesPublicEvents();


            Transporter.PullResponseFactory = (r) => new HttpPullResponse()
            {
                StatusCode = GSStatusCode.OK,
                Streams = CreateStreams(new UsernameSet(new SetUsername(u.Id, remoteName))
                {
                    AggregateVersion = 2,
                    Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 20),
                    MessageId = Guid.NewGuid()
                },
                new BecameFollower(new BecomeFollower(u.Id, remoteTarget))
                {
                    AggregateVersion = 3,
                    Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 15),
                    MessageId = Guid.NewGuid()
                },
                new UsernameSet(new SetUsername(u.Id, newerRemoteName))
                {
                    AggregateVersion = 4,
                    Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 10),
                    MessageId = Guid.NewGuid()
                }
                )

            };

            App.Bus.SendCommands(
                new SetUsername(u.Id, localName),
                new BecomeFollower(u.Id, remoteTarget),
                new SetUsername(u.Id, newerLocalName)
            );

            var R = WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(3, pushStream.Count);
            Assert.IsInstanceOf<UsernameSet>(pushStream.Single());


            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(5, translated.Single().AggregateVersion);

            //var pullStream = R.PullResp.Streams.Single();
            //Assert.AreEqual(1, pullStream.Count);
            //Assert.IsInstanceOf<INullEvent>(pullStream.Single());


            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(5, User.Version);

            Assert.AreEqual(localName, User.State.Username);



        }


        [Test]
        public void TestPhotoUpload()
        {

            // ARRANGE
            var u = App.Context.CurrentUser;
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


            App.Bus.SendCommand(new CreatePlant(plantId, "Jare", u.GardenId, u.Id));
            App.Bus.SendCommand(new CreatePlantAction(plantActionId, u.Id, plantId, PlantActionType.PHOTOGRAPHED, "Just a photo")
                {
                    Photo = photo
                }
            );
            App.Bus.SendCommand(new SchedulePhotoUpload(photo));


            Assert.AreEqual(photo, App.Model.State.PhotoUploads.Values.Single());


            var R = WaitForTask(App.Synchronize());

            Assert.IsNotNull(R.PhotoUploadRequests);

            Assert.AreEqual(0, App.Model.State.PhotoUploads.Count);


        }

        [Test]
        public void TestPhotoDownload()
        {

            // ARRANGE
            var u = App.Context.CurrentUser;
            var plantId = Guid.NewGuid();
            var plantActionId = Guid.NewGuid();



            var photo = new Photo()
            {
                RemoteUri = "http://plantimages.com/image1.jpg"

            };


            App.Bus.SendCommand(new CreatePlant(plantId, "Jare", u.GardenId, u.Id));


            Transporter.PullResponseFactory = (r) => new HttpPullResponse()
            {
                StatusCode = GSStatusCode.OK,
                Streams = CreateStreams(new PlantActionCreated(new CreatePlantAction(plantActionId, u.Id, plantId, PlantActionType.PHOTOGRAPHED, "Just a photo")
                {
                    Photo = photo
                })
                {
                    AggregateVersion = 1,
                    Created = DateTimeOffset.UtcNow - new TimeSpan(0, 0, 20),
                    MessageId = Guid.NewGuid()
                }
                )

            };

            var R = WaitForTask(App.Synchronize());


            Assert.AreEqual(0, App.Model.State.PhotoDownloads.Count);

            var PhotoAction = (PlantAction)Repository.GetById(plantActionId);

            Assert.AreEqual(photo, PhotoAction.State.Photo);

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

    }
}
