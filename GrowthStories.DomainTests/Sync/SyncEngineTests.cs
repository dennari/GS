using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using Ninject;
//using NUnit.Framework;
#if WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.Storage;

#else
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endif
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;


namespace Growthstories.DomainTests
{


    [TestClass]
    public class SyncEngineTests
    {

        IKernel Kernel;
        IGSAppViewModel App;
        GSAppState AppState;
        IGSRepository Repository;
        FakeHttpClient Transporter;

        [TestInitialize]
        public void SetUp()
        {

            RxApp.InUnitTestRunnerOverride = true;
            //if (kernel != null)
            //    kernel.Dispose();
            Kernel = new StandardKernel(new SyncEngineTestsSetup());

            var app = new TestAppViewModel(Kernel);
            this.App = app;
            this.AppState = app.Model.State;
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

        [TestMethod]
        public void TestPushIncludesPublicEvents()
        {
            var u = App.Context.CurrentUser;
            App.HandleCommand(new CreateUser(u.Id, u.Username, u.Password, u.Email));
            App.HandleCommand(new AssignAppUser(u.Id, u.Username, u.Password, u.Email));


            var R = TestUtils.WaitForTask(App.Synchronize());

            Assert.IsFalse(R.PushReq.IsEmpty);

            Assert.AreEqual(u.Id, R.PushReq.Streams.Single().Single().AggregateId);

        }

        [TestMethod]
        public void TestCreatedUserIsInPullRequest()
        {
            var u = App.Context.CurrentUser;
            App.HandleCommand(new CreateUser(u.Id, u.Username, u.Password, u.Email));
            App.HandleCommand(new AssignAppUser(u.Id, u.Username, u.Password, u.Email));


            var R = TestUtils.WaitForTask(App.Synchronize());

            Assert.IsFalse(R.PullReq.IsEmpty);
            Assert.AreEqual(u.Id, R.PullReq.Streams.Single().StreamId);
        }




        public static IStreamSegment[] CreateStreams(params IMessage[] msgs)
        {
            return msgs.GroupBy(x => x.AggregateId).Select(x => new StreamSegment(x)).ToArray();
        }

        [TestMethod]
        public void TestPullCanCreateStream()
        {

            var remoteUser = TestUtils.CreateUserFromName("Jorma");

            Transporter.PullResponseFactory = (r) => new HttpPullResponse()
            {
                StatusCode = GSStatusCode.OK,
                Streams = CreateStreams(remoteUser)
            };

            App.HandleCommand(new CreateSyncStream(remoteUser.AggregateId, PullStreamType.USER));


            var R = TestUtils.WaitForTask(App.Synchronize());

            var RemoteUser = Repository.GetById(remoteUser.AggregateId);
            Assert.IsInstanceOfType(RemoteUser, typeof(User));
            Assert.AreEqual(1, RemoteUser.Version);

        }



        [TestMethod]
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

            App.HandleCommand(new BecomeFollower(u.Id, localTarget));

            var R = TestUtils.WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);

            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(3, translated[0].AggregateVersion);


            var User = Repository.GetById(u.Id);
            Assert.IsInstanceOfType(User, typeof(User));
            Assert.AreEqual(3, User.Version);

        }


        [TestMethod]
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

            App.HandleCommand(new SetUsername(u.Id, localName));

            var R = TestUtils.WaitForTask(App.Synchronize());

            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);
            Assert.IsInstanceOfType(pushStream.Single(), typeof(INullEvent));

            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(3, translated.Single().AggregateVersion);


            var pullStream = R.PullResp.Streams.Single();
            Assert.AreEqual(1, pullStream.Count);
            Assert.IsInstanceOfType(pullStream.Single(), typeof(UsernameSet));

            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(3, User.Version);

            Assert.AreEqual(remoteName, User.State.Username);



        }



        [TestMethod]
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

            App.HandleCommand(new SetUsername(u.Id, localName));

            var R = TestUtils.WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);
            Assert.IsInstanceOfType(pushStream.Single(), typeof(UsernameSet));


            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(3, translated.Single().AggregateVersion);

            var pullStream = R.PullResp.Streams.Single();
            Assert.AreEqual(1, pullStream.Count);
            Assert.IsInstanceOfType(pullStream.Single(), typeof(INullEvent));


            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(3, User.Version);

            Assert.AreEqual(localName, User.State.Username);



        }



        [TestMethod]
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

            App.HandleCommand(new SetUsername(u.Id, localName));

            var R = TestUtils.WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);
            Assert.IsInstanceOfType(pushStream.Single(), typeof(INullEvent));


            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(5, translated.Single().AggregateVersion);

            //var pullStream = R.PullResp.Streams.Single();
            //Assert.AreEqual(1, pullStream.Count);
            //Assert.IsInstanceOf<INullEvent>(pullStream.Single());


            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(5, User.Version);

            Assert.AreEqual(newerRemoteName, User.State.Username);



        }


        [TestMethod]
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

            App.HandleCommand(new SetUsername(u.Id, localName));

            var R = TestUtils.WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(1, pushStream.Count);
            Assert.IsInstanceOfType(pushStream.Single(), typeof(INullEvent));


            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(5, translated.Single().AggregateVersion);

            //var pullStream = R.PullResp.Streams.Single();
            //Assert.AreEqual(1, pullStream.Count);
            //Assert.IsInstanceOf<INullEvent>(pullStream.Single());


            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(5, User.Version);

            Assert.AreEqual(newerRemoteName, User.State.Username);



        }



        [TestMethod]
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

            App.HandleCommands(
                new SetUsername(u.Id, localName),
                new BecomeFollower(u.Id, remoteTarget),
                new SetUsername(u.Id, newerLocalName)
            );

            var R = TestUtils.WaitForTask(App.Synchronize());
            //Assert.IsTrue(R.PushReq.IsEmpty);
            var pushStream = R.PushReq.Streams.Single();
            Assert.AreEqual(3, pushStream.Count);
            Assert.IsInstanceOfType(pushStream.Single(), typeof(UsernameSet));

            var translated = ((HttpPushRequest)R.PushReq).Events.ToArray();
            Assert.AreEqual(5, translated.Single().AggregateVersion);

            //var pullStream = R.PullResp.Streams.Single();
            //Assert.AreEqual(1, pullStream.Count);
            //Assert.IsInstanceOf<INullEvent>(pullStream.Single());


            var User = (User)Repository.GetById(u.Id);
            Assert.AreEqual(5, User.Version);

            Assert.AreEqual(localName, User.State.Username);



        }


        [TestMethod]
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


            App.HandleCommand(new CreatePlant(plantId, "Jare", u.GardenId, u.Id));
            App.HandleCommand(new CreatePlantAction(plantActionId, u.Id, plantId, PlantActionType.PHOTOGRAPHED, "Just a photo")
                {
                    Photo = photo
                }
            );
            App.HandleCommand(new SchedulePhotoUpload(photo));


            Assert.AreEqual(photo, AppState.PhotoUploads.Values.Single());


            var R = TestUtils.WaitForTask(App.Synchronize());

            Assert.IsNotNull(R.PhotoUploadRequests);

            Assert.AreEqual(0, AppState.PhotoUploads.Count);


        }

        [TestMethod]
        public void TestPhotoDownload()
        {

            // ARRANGE
            var u = App.Context.CurrentUser;
            var plantId = Guid.NewGuid();
            var plantActionId = Guid.NewGuid();



            var photo = new Photo()
            {
                BlobKey = "skdjdlgdfg",
                LocalFullPath = @"C:\Users\Ville\Documents\Visual Studio 2012\Projects\GrowthStories\GrowthStories.DomainTests\Data\img_download_test.jpg",
                LocalUri = "img_download_test.jpg",
                FileName = "img_download_test.jpg",
                Width = 500,
                Height = 500,
                Size = 500 * 500 * 3
            };

#if WINDOWS_PHONE
            try
            {
                var folderTask = ApplicationData.Current.LocalFolder.CreateFolderAsync(WP8PhotoHandler.IMG_FOLDER, CreationCollisionOption.OpenIfExists);
                var folder = TestUtils.WaitForTask(folderTask.AsTask());
                TestUtils.WaitForTask(folder.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask());


            }
            catch { }
#else
            try
            {
                File.Delete(photo.LocalFullPath);
            }
            catch { }
#endif


            App.HandleCommand(new CreatePlant(plantId, "Jare", u.GardenId, u.Id));


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

            Transporter.PhotoDownloadUriResponseFactory = (blobKey) => new PhotoUriResponse()
            {
                StatusCode = GSStatusCode.OK,
                PhotoUri = new Uri("http://upload.wikimedia.org/wikipedia/commons/e/e3/CentaureaCyanus-bloem-kl.jpg")
            };

            Transporter.PhotoDownloadResponseFactory = (req) =>
            {
                var client = Kernel.Get<SyncHttpClient>();
                return client.RequestPhotoDownload(req);

            };

            var R = TestUtils.WaitForTask(App.Synchronize());


            Assert.AreEqual(0, AppState.PhotoDownloads.Count);

            var PhotoAction = (PlantAction)Repository.GetById(plantActionId);

            Assert.AreEqual(photo, PhotoAction.State.Photo);



#if WINDOWS_PHONE
            var folderFound = false;
            var fileFound = false;
            try
            {
                var folderTask = ApplicationData.Current.LocalFolder.CreateFolderAsync(WP8PhotoHandler.IMG_FOLDER, CreationCollisionOption.OpenIfExists);
                var folder = TestUtils.WaitForTask(folderTask.AsTask());
                folderFound = true;
                var file = TestUtils.WaitForTask(folder.CreateFileAsync(photo.FileName, CreationCollisionOption.FailIfExists).AsTask());
                Assert.Fail("Image didn't exist");

            }
            catch
            {
                fileFound = true;

            }
            Assert.IsTrue(folderFound);
            Assert.IsTrue(fileFound);
#else
            Assert.IsTrue(File.Exists(photo.LocalFullPath));
#endif

        }





    }
}
