using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventStore;
using EventStore.Persistence.SqlPersistence;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Growthstories.UI.Persistence;
using Growthstories.UI.ViewModel;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone.ViewModels
{

    public sealed class ClientTestingViewModel : TestingViewModel
    {
        private readonly IDispatchCommands Handler;
        private SQLitePersistenceEngine Store;
        private SQLiteUIPersistence UIStore;
        private GSRepository Repo;
        private OptimisticPipelineHook Pipelinehook;
        private ITranslateEvents Translator;
        private ITransportEvents Transporter;
        private IHttpClient HttpClient;
        private IJsonFactory Serializer;


        public ClientTestingViewModel(
            SQLitePersistenceEngine store,
            SQLiteUIPersistence uiStore,
            GSRepository repo,
            OptimisticPipelineHook pipelinehook,
            IDispatchCommands handler,
            ITranslateEvents translator,
            ITransportEvents transporter,
            IHttpClient httpclient,
            IJsonFactory serializer,
            IGSAppViewModel app)
            : base(app)
        {

            this.Handler = handler;
            this.Store = store;
            this.UIStore = uiStore;
            this.Repo = repo;
            this.Pipelinehook = pipelinehook;
            this.Translator = translator;
            this.Transporter = transporter;
            this.HttpClient = httpclient;
            this.Serializer = serializer;

            this.CreateRemoteDataCommand.Subscribe(x => this.CreateRemoteTestData());
            this.CreateLocalDataCommand.RegisterAsyncTask(o => Task.Run(() => CreateLocalTestData())).Publish().Connect();
            this.PushRemoteUserCommand.RegisterAsyncTask(o => PushRemoteUser()).Publish().Connect();
            this.SyncCommand.RegisterAsyncTask(_ => SyncAll()).Publish().Connect();
            //this.PushCommand.RegisterAsyncTask(_ => PushAll()).Publish().Connect();

            this.ClearDBCommand.Subscribe(_ => this.ClearDB());

        }

        private async Task<bool> SyncAll()
        {
            int counter = 0;
            var R = await App.Synchronize();
            if (R == null || R.Item1 == AllSyncResult.AllSynced)
            {
                return true;
            }
            return false;
        }

        //private async Task<bool> PushAll()
        //{
        //    int maxRounds = 100;
        //    int counter = 0;
        //    ISyncInstance R = null;
        //    while (counter < maxRounds)
        //    {
        //        R = await App.Push();
        //        if (R == null || R.PushReq.IsEmpty || R.PushResp.StatusCode != GSStatusCode.OK)
        //            return true;
        //    }
        //    return false;
        //}

        public void CreateLocalTestData()
        {

            if (App.User == null)
            {
                App.WhenAny(x => x.User, x => x.GetValue()).Where(x => x != null).Take(1).Subscribe(_ => _CreateLocalTestData());
            }
            else
            {
                _CreateLocalTestData2();
            }
        }


        private void _CreateLocalTestData2()
        {

            for (var i = 0; i < 30; i++)
            {

                var localPlant = new CreatePlant(Guid.NewGuid(), "Jare" + i, App.User.GardenId, App.User.Id);
                Handler.Handle(localPlant);

                var localPlantProperty = new MarkPlantPublic(localPlant.AggregateId);
                Handler.Handle(localPlantProperty);

                Handler.Handle(new AddPlant(App.User.GardenId, localPlant.AggregateId, App.User.Id, "Jare " + i));

                //var wateringSchedule = new CreateSchedule(Guid.NewGuid(), 24 * 2 * 3600);
                //Handler.Handle(wateringSchedule);
                //Handler.Handle(new SetWateringSchedule(localPlant.AggregateId, wateringSchedule.AggregateId));

                //var FertilizingSchedule = new CreateSchedule(Guid.NewGuid(), 24 * 50 * 3600);
                //Handler.Handle(FertilizingSchedule);
                //Handler.Handle(new SetFertilizingSchedule(localPlant.AggregateId, FertilizingSchedule.AggregateId));

                Handler.Handle(
                        new CreatePlantAction(
                            Guid.NewGuid(),
                            App.User.Id,
                            localPlant.AggregateId,
                            PlantActionType.COMMENTED,
                            "Hello local world " + i));

                //Handler.Handle(
                //    new CreatePlantAction(
                //        Guid.NewGuid(),
                //        App.User.Id,
                //        localPlant.AggregateId,
                //        PlantActionType.PHOTOGRAPHED,
                //        "Hello local world " + i)
                //    {
                //        Photo = new Photo()
                //        {
                //            RemoteUri = @"http://upload.wikimedia.org/wikipedia/commons/e/e3/CentaureaCyanus-bloem-kl.jpg"
                //        }
                //    });

                Handler.Handle(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.User.Id,
                        localPlant.AggregateId,
                        PlantActionType.FERTILIZED,
                        "Hello local world " + i));

                for (int j = 0; j < 20; j++)
                {
                    Handler.Handle(
                        new CreatePlantAction(
                            Guid.NewGuid(),
                            App.User.Id,
                            localPlant.AggregateId,
                            PlantActionType.WATERED,
                            "Hello local world " + j));
                }

                //IPhoto photo = null;
                //try
                //{
                //    photo = App.MyGarden.Plants.First().Photo;
                //}
                //catch { }
                
                //Handler.Handle(
                //    new CreatePlantAction(
                //        Guid.NewGuid(),
                //        App.User.Id,
                //        localPlant.AggregateId,
                //        PlantActionType.PHOTOGRAPHED,
                //        "Hello local world " + i)
                //    {
                //        Photo = photo
                //    });

            }


        }


        private void _CreateLocalTestData()
        {

            for (var i = 0; i < 5; i++)
            {

                var localPlant = new CreatePlant(Guid.NewGuid(), "RemoteJare " + i, App.User.GardenId, App.User.Id);
                Handler.Handle(localPlant);


                //var photoPath = i % 2 == 0 ? @"/TestData/flowers-from-the-conservatory.jpg" : @"/TestData/517e100d782a828894.jpg";
                //App.Bus.SendCommand(new SetProfilepicture(localPlant.AggregateId, new Photo()
                //    {
                //        LocalUri = photoPath,
                //        LocalFullPath = photoPath
                //    }
                //));

                var localPlantProperty = new MarkPlantPublic(localPlant.AggregateId);
                Handler.Handle(localPlantProperty);

                Handler.Handle(new AddPlant(App.User.GardenId, localPlant.AggregateId, App.User.Id, "Jare " + i));


                var wateringSchedule = new CreateSchedule(Guid.NewGuid(), 24 * 2 * 3600);
                Handler.Handle(wateringSchedule);
                Handler.Handle(new SetWateringSchedule(localPlant.AggregateId, wateringSchedule.AggregateId));

                var FertilizingSchedule = new CreateSchedule(Guid.NewGuid(), 24 * 50 * 3600);
                Handler.Handle(FertilizingSchedule);
                Handler.Handle(new SetFertilizingSchedule(localPlant.AggregateId, FertilizingSchedule.AggregateId));

                Handler.Handle(
                        new CreatePlantAction(
                            Guid.NewGuid(),
                            App.User.Id,
                            localPlant.AggregateId,
                            PlantActionType.COMMENTED,
                            "Hello local world " + i));


                Handler.Handle(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.User.Id,
                        localPlant.AggregateId,
                        PlantActionType.PHOTOGRAPHED,
                        "Hello local world " + i)
                    {
                        Photo = new Photo()
                        {
                            RemoteUri = @"http://upload.wikimedia.org/wikipedia/commons/e/e3/CentaureaCyanus-bloem-kl.jpg"
                        }
                    });

                Handler.Handle(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.User.Id,
                        localPlant.AggregateId,
                        PlantActionType.FERTILIZED,
                        "Hello local world " + i));

                Handler.Handle(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.User.Id,
                        localPlant.AggregateId,
                        PlantActionType.WATERED,
                        "Hello local world " + i));

                Handler.Handle(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.User.Id,
                        localPlant.AggregateId,
                        PlantActionType.PHOTOGRAPHED,
                        "Hello local world " + i)
                    {
                        Photo = new Photo()
                        {
                            RemoteUri = @"http://upload.wikimedia.org/wikipedia/commons/d/d3/Nelumno_nucifera_open_flower_-_botanic_garden_adelaide2.jpg"
                        }
                    });

                Handler.Handle(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.User.Id,
                        localPlant.AggregateId,
                        PlantActionType.PHOTOGRAPHED,
                        "Hello local world " + i)
                    {
                        Photo = new Photo()
                        {
                            RemoteUri = @"http://upload.wikimedia.org/wikipedia/commons/6/66/White_Flower_Closeup.jpg"
                        }
                    });


            }


        }

        private UserCreated CreateUserFromName(string name)
        {
            return new UserCreated(new CreateUser(
                Guid.NewGuid(),
                name,
                "swordfish",
                name + Guid.NewGuid() + "@wonderland.net"))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };
        }

        public void CreateRemoteTestData()
        {
            // create remote data

            var name = "Lauri";

            var remoteUser = new CreateUser(
                Guid.NewGuid(),
                name,
                "swordfish",
                name + "@wonderland.net");
            Handler.Handle(remoteUser);
            //var pushResp = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            //{
            //    Streams = EventsToStreams(remoteUser.AggregateId, remoteUser),
            //    ClientDatabaseId = Guid.NewGuid(),
            //    Translator = Translator
            //});


            var remoteGarden = new CreateGarden(Guid.NewGuid(), remoteUser.AggregateId);
            Handler.Handle(remoteGarden);


            var remoteAddGarden = new AddGarden(remoteUser.AggregateId, remoteGarden.EntityId.Value);
            Handler.Handle(remoteAddGarden);

            for (var i = 0; i < 5; i++)
            {

                var remotePlant = new CreatePlant(Guid.NewGuid(), "RemoteJare " + i, remoteGarden.EntityId.Value, remoteUser.AggregateId);
                Handler.Handle(remotePlant);

                var remotePlantProperty = new MarkPlantPublic(remotePlant.AggregateId);
                Handler.Handle(remotePlantProperty);


                var remoteAddPlant = new AddPlant(remoteGarden.EntityId.Value, remotePlant.AggregateId, remoteUser.AggregateId, "RemoteJare " + i);
                Handler.Handle(remoteAddPlant);

                var remoteComment =
                        new CreatePlantAction(
                            Guid.NewGuid(),
                            remoteUser.AggregateId,
                            remotePlant.AggregateId,
                            PlantActionType.COMMENTED,
                            "Hello remote world " + i);

                Handler.Handle(remoteComment);

                var remotePhoto =
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        remoteUser.AggregateId,
                        remotePlant.AggregateId,
                        PlantActionType.PHOTOGRAPHED,
                        "Hello remote world " + i)
                    {
                        Photo = new Photo()
                        {
                            RemoteUri = "http://upload.wikimedia.org/wikipedia/commons/e/e3/CentaureaCyanus-bloem-kl.jpg"
                        }
                    };

                Handler.Handle(remotePhoto);
            }

        }

        private async Task PushRemoteUser()
        {
            // create remote data



            var name = "Lauri";

            var remoteUser = CreateUserFromName(name);

            var pushResp = await Transporter.PushAsync(new HttpPushRequest(Serializer)
            {
                Streams = new[] { new StreamSegment(remoteUser.AggregateId, remoteUser) },
                ClientDatabaseId = Guid.NewGuid(),
                Translator = Translator
            });
            //Assert.AreEqual(GSStatusCode.OK, pushResp.StatusCode);

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
                remotePlant,
                remotePlantProperty,
                remoteComment,
                remoteGarden, 
                remoteAddGarden,                
                remoteAddPlant
            };





            var authResponse = await Transporter.RequestAuthAsync(remoteUser.Email, remoteUser.Password);

            //Assert.AreEqual(authResponse.StatusCode, GSStatusCode.OK);
            //this.RemoteAuth = authResponse.AuthToken;

            await PushAsAnother(authResponse.AuthToken, remoteEvents);

            //remoteEvents.Insert(0, remoteUser);

            //return remoteEvents;
        }

        public async Task PushAsAnother(IAuthToken anotherAuth, IEnumerable<IEvent> events)
        {
            var tmp = HttpClient.AuthToken;
            HttpClient.AuthToken = anotherAuth;

            ISyncPushResponse R = null;
            foreach (var e in events)
            {
                R = await Transporter.PushAsync(new HttpPushRequest(Serializer)
                {
                    Streams = new IStreamSegment[] { new StreamSegment(e.AggregateId, e) },
                    ClientDatabaseId = Guid.NewGuid(),
                    Translator = Translator
                });
                //Assert.AreEqual(GSStatusCode.OK, R.StatusCode);

            }

            HttpClient.AuthToken = tmp;

        }


        private IStreamSegment[] EventsToStreams(Guid aggregateId, IEvent events)
        {
            var msgs = new StreamSegment(aggregateId);
            msgs.Add(events);
            return new[] { msgs };
        }

        private IStreamSegment[] EventsToStreams(Guid aggregateId, IEnumerable<IEvent> events)
        {
            var msgs = new StreamSegment(aggregateId);
            msgs.AddRange(events);
            return new[] { msgs };
        }

        private IEnumerable<IStreamSegment> EventsToStreams(IEnumerable<IEvent> events)
        {

            foreach (var g in events.GroupBy(x => x.AggregateId))
            {
                var msgs = new StreamSegment(g.Key);
                msgs.AddRange(g);
                yield return msgs;
            }

        }

        public void ClearDB()
        {


            Store.ReInitialize();

            UIStore.ReInitialize();

            Repo.ClearCaches();

            Pipelinehook.Dispose();

            //((ClientAppViewModel)App).ResetUI();

            //App.Router.NavigateAndReset.Execute(new MainViewModel(App));

            App.ShowPopup.Execute(new PopupViewModel()
            {
                Caption = "DB cleared, restart app"
            });
        }




    }
}
