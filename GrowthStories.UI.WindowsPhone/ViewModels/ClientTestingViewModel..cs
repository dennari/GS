using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Tasks;
using Growthstories.Sync;
using Growthstories.Domain;
using Growthstories.UI;
using System.Windows.Media.Imaging;
using System.IO;
using Windows.Storage.Streams;
using Growthstories.UI.WindowsPhone;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;

using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using System.Linq.Expressions;
using Ninject;
using Growthstories.Core;
using EventStore;
using EventStore.Persistence;

namespace Growthstories.UI.WindowsPhone.ViewModels
{

    public sealed class ClientTestingViewModel : TestingViewModel
    {
        private readonly IKernel Kernel;
        private readonly IDispatchCommands Handler;

        public ClientTestingViewModel(IKernel kernel, IGSAppViewModel app)
            : base(app)
        {

            this.Kernel = kernel;
            this.Handler = Kernel.Get<IDispatchCommands>();

            this.CreateRemoteDataCommand.Subscribe(x => this.CreateRemoteTestData());
            this.CreateLocalDataCommand.Subscribe(x => this.CreateLocalTestData());

        }

        public void CreateLocalTestData()
        {



            for (var i = 0; i < 5; i++)
            {

                var localPlant = new CreatePlant(Guid.NewGuid(), "RemoteJare " + i, App.Context.CurrentUser.GardenId, App.Context.CurrentUser.Id);
                App.Bus.SendCommand(localPlant);

                var localPlantProperty = new MarkPlantPublic(localPlant.AggregateId);
                App.Bus.SendCommand(localPlantProperty);

                App.Bus.SendCommand(new AddPlant(App.Context.CurrentUser.GardenId, localPlant.AggregateId, App.Context.CurrentUser.Id, "Jare " + i));


                var wateringSchedule = new CreateSchedule(Guid.NewGuid(), 24 * 2 * 3600);
                App.Bus.SendCommand(wateringSchedule);
                App.Bus.SendCommand(new SetWateringSchedule(localPlant.AggregateId, wateringSchedule.AggregateId));

                var FertilizingSchedule = new CreateSchedule(Guid.NewGuid(), 24 * 50 * 3600);
                App.Bus.SendCommand(FertilizingSchedule);
                App.Bus.SendCommand(new SetFertilizingSchedule(localPlant.AggregateId, FertilizingSchedule.AggregateId));

                App.Bus.SendCommand(
                        new CreatePlantAction(
                            Guid.NewGuid(),
                            App.Context.CurrentUser.Id,
                            localPlant.AggregateId,
                            PlantActionType.COMMENTED,
                            "Hello local world " + i));


                App.Bus.SendCommand(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.Context.CurrentUser.Id,
                        localPlant.AggregateId,
                        PlantActionType.PHOTOGRAPHED,
                        "Hello local world " + i)
                    {
                        Photo = new Photo()
                        {
                            RemoteUri = @"http://upload.wikimedia.org/wikipedia/commons/e/e3/CentaureaCyanus-bloem-kl.jpg"
                        }
                    });

                App.Bus.SendCommand(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.Context.CurrentUser.Id,
                        localPlant.AggregateId,
                        PlantActionType.FERTILIZED,
                        "Hello local world " + i));

                App.Bus.SendCommand(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.Context.CurrentUser.Id,
                        localPlant.AggregateId,
                        PlantActionType.WATERED,
                        "Hello local world " + i));

                App.Bus.SendCommand(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.Context.CurrentUser.Id,
                        localPlant.AggregateId,
                        PlantActionType.PHOTOGRAPHED,
                        "Hello local world " + i)
                    {
                        Photo = new Photo()
                        {
                            RemoteUri = @"http://upload.wikimedia.org/wikipedia/commons/d/d3/Nelumno_nucifera_open_flower_-_botanic_garden_adelaide2.jpg"
                        }
                    });

                App.Bus.SendCommand(
                    new CreatePlantAction(
                        Guid.NewGuid(),
                        App.Context.CurrentUser.Id,
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
                name + "@wonderland.net"))
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
            App.Bus.SendCommand(remoteUser);
            //var pushResp = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            //{
            //    Streams = EventsToStreams(remoteUser.AggregateId, remoteUser),
            //    ClientDatabaseId = Guid.NewGuid(),
            //    Translator = Translator
            //});


            var remoteGarden = new CreateGarden(Guid.NewGuid(), remoteUser.AggregateId);
            App.Bus.SendCommand(remoteGarden);


            var remoteAddGarden = new AddGarden(remoteUser.AggregateId, remoteGarden.EntityId.Value);
            App.Bus.SendCommand(remoteAddGarden);

            for (var i = 0; i < 5; i++)
            {

                var remotePlant = new CreatePlant(Guid.NewGuid(), "RemoteJare " + i, remoteGarden.EntityId.Value, remoteUser.AggregateId);
                App.Bus.SendCommand(remotePlant);

                var remotePlantProperty = new MarkPlantPublic(remotePlant.AggregateId);
                App.Bus.SendCommand(remotePlantProperty);


                var remoteAddPlant = new AddPlant(remoteGarden.EntityId.Value, remotePlant.AggregateId, remoteUser.AggregateId, "RemoteJare " + i);
                App.Bus.SendCommand(remoteAddPlant);

                var remoteComment =
                        new CreatePlantAction(
                            Guid.NewGuid(),
                            remoteUser.AggregateId,
                            remotePlant.AggregateId,
                            PlantActionType.COMMENTED,
                            "Hello remote world " + i);

                App.Bus.SendCommand(remoteComment);

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

                App.Bus.SendCommand(remotePhoto);
            }

        }

        private async Task PushAsAnother(IAuthToken anotherAuth, IEnumerable<IEvent> events)
        {
            var tmp = HttpClient.AuthToken;
            HttpClient.AuthToken = anotherAuth;

            var pushResp2 = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Streams = EventsToStreams(events).ToArray(),
                ClientDatabaseId = Guid.NewGuid(),
                Translator = Translator
            });



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

            var db = Kernel.Get<IPersistSyncStreams>();
            db.Purge();
            //Kernel.Get<IGSRepository>().ClearCaches();
            Kernel.Get<OptimisticPipelineHook>().Dispose();
            //((FakeUserService)Kernel.Get<IUserService>()).EnsureCurrenUser();

        }

        public T Get<T>() { return Kernel.Get<T>(); }
        //public IMessageBus Handler { get { return Get<IMessageBus>(); } }


        public ISynchronizerService Synchronizer { get { return Get<ISynchronizerService>(); } }
        //public IStoreSyncHeads SyncStore { get { return Get<IStoreSyncHeads>(); } }
        public IRequestFactory RequestFactory { get { return Get<IRequestFactory>(); } }

        public ITransportEvents Transporter { get { return Get<ITransportEvents>(); } }
        public ITranslateEvents Translator { get { return Get<ITranslateEvents>(); } }
        //public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        // public IGSRepository Repository { get { return Get<IGSRepository>(); } }
        // public GSEventStore EventStore { get { return (GSEventStore)Get<IStoreEvents>(); } }

        // public IUserService UserService { get { return Get<IUserService>(); } }

        public SyncHttpClient HttpClient { get { return (SyncHttpClient)Kernel.Get<IHttpClient>(); } }
    }
}
