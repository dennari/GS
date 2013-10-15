
using ReactiveUI;
using ReactiveUI.Mobile;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using Ninject;
using Growthstories.UI.WindowsPhone;
using Growthstories.UI.ViewModel;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain;
using System.Threading.Tasks;
using Growthstories.Domain.Messaging;
using Growthstories.UI.WindowsPhone.ViewModels;
using EventStore.Persistence;
using EventStore;


namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public sealed class AppViewModel : Growthstories.UI.ViewModel.AppViewModel, IApplicationRootState
    {



        protected Microsoft.Phone.Controls.SupportedPageOrientation _ClientSupportedOrientations;
        public Microsoft.Phone.Controls.SupportedPageOrientation ClientSupportedOrientations
        {
            get
            {
                return _ClientSupportedOrientations;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ClientSupportedOrientations, value);
            }
        }



        public AppViewModel()
            : base()
        {
            if (DesignModeDetector.IsInDesignMode())
            {
                // Create design time view services and models
                this.Kernel = new StandardKernel(new BootstrapDesign());
            }
            else
            {
                // Create run time view services and models
                this.Kernel = new StandardKernel(new Bootstrap());
            }
            Kernel.Bind<IScreen>().ToConstant(this);
            Kernel.Bind<IRoutingState>().ToConstant(this.Router);
            this.Bus = Kernel.Get<IMessageBus>();

            Resolver.RegisterLazySingleton(() => new MainView(), typeof(IViewFor<MainViewModel>));
            Resolver.RegisterLazySingleton(() => new GardenViewPage(), typeof(IViewFor<GardenViewModel>));
            Resolver.RegisterLazySingleton(() => new PlantView(), typeof(IViewFor<PlantViewModel>));
            Resolver.RegisterLazySingleton(() => new ScheduleView(), typeof(IViewFor<ScheduleViewModel>));
            Resolver.RegisterLazySingleton(() => new AddPlantView(), typeof(IViewFor<ClientAddPlantViewModel>));
            //Resolver.RegisterLazySingleton(() => new EditPlantView(), typeof(IViewFor<ClientEditPlantViewModel>));
            Resolver.RegisterLazySingleton(() => new AddWaterView(), typeof(IViewFor<PlantWaterViewModel>));
            Resolver.RegisterLazySingleton(() => new AddCommentView(), typeof(IViewFor<PlantCommentViewModel>));
            Resolver.RegisterLazySingleton(() => new AddFertilizerView(), typeof(IViewFor<PlantFertilizeViewModel>));
            Resolver.RegisterLazySingleton(() => new AddMeasurementView(), typeof(IViewFor<PlantMeasureViewModel>));
            Resolver.RegisterLazySingleton(() => new AddPhotographView(), typeof(IViewFor<ClientPlantPhotographViewModel>));
            Resolver.RegisterLazySingleton(() => new YAxisShitView(), typeof(IViewFor<YAxisShitViewModel>));
            Resolver.RegisterLazySingleton(() => new ListUsersView(), typeof(IViewFor<ListUsersViewModel>));


            this.WhenAny(x => x.SupportedOrientations, x => x.GetValue()).Subscribe(x => this.ClientSupportedOrientations = (Microsoft.Phone.Controls.SupportedPageOrientation)x);


            this._Model = Initialize(Kernel.Get<IGSRepository>());



        }

        public override AddPlantViewModel AddPlantViewModelFactory(PlantState state)
        {
            return new ClientAddPlantViewModel(state, this);
        }

        public override IPlantActionViewModel PlantActionViewModelFactory<T>(PlantActionState state = null)
        {
            if ((state != null && state.Type == PlantActionType.PHOTOGRAPHED) || typeof(T) == typeof(IPlantPhotographViewModel))
                return new ClientPlantPhotographViewModel(state, this);
            else
                return base.PlantActionViewModelFactory<T>(state);
        }

        public override Task AddTestData()
        {

            return Task.Run(async () =>
            {


                Handler.Handle(SetIds(new CreatePlant(Guid.NewGuid(), "Jore", Context.CurrentUser.GardenId, Context.CurrentUser.Id)
                {
                    Profilepicture = new Photo()
                    {
                        LocalUri = "/TestData/517e100d782a828894.jpg"
                    }
                }));


                Handler.Handle(SetIds(new CreatePlant(Guid.NewGuid(), "Jari", Context.CurrentUser.GardenId, Context.CurrentUser.Id)
                {
                    Profilepicture = new Photo()
                    {
                        LocalUri = "/TestData/flowers-from-the-conservatory.jpg"
                    }
                }));

                var client = Kernel.Get<SyncHttpClient>();
                await client.SendAsync(client.CreateClearDBRequest());
                await Kernel.Get<ISynchronizerService>().CreateUserAsync(Context.CurrentUser.Id);


                await Context.AuthorizeUser();

                await CreateRemoteData();

                //var remoteUser = new CreateUser(Guid.NewGuid(), "RemoUser", "1234", "user@gs.com");
                //Handler.Handle(SetIds(remoteUser));

                //var remoteGarden = new CreateGarden(Guid.NewGuid(), remoteUser.AggregateId);
                //Handler.Handle(SetIds(SetIds(remoteGarden, null, remoteUser.AggregateId)));

                //Handler.Handle(SetIds(new AddGarden(remoteUser.AggregateId, remoteGarden.EntityId.Value), null, remoteUser.AggregateId));


                //var remotePlant = SetIds(new CreatePlant(Guid.NewGuid(), "RemoteJare", remoteGarden.EntityId.Value, remoteUser.AggregateId), null, remoteUser.AggregateId);
                //Handler.Handle(remotePlant);

                //Handler.Handle(SetIds(new MarkPlantPublic(remotePlant.AggregateId), null, remoteUser.AggregateId));




            });
        }

        protected UserCreated CreateUserFromName(string name)
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

        protected async Task<List<IEvent>> CreateRemoteData()
        {
            // create remote data



            var remoteUser = CreateUserFromName("Lauri");

            var pushResp = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Events = Translator.Out(new IEvent[] { remoteUser }),
                ClientDatabaseId = Guid.NewGuid()
            });


            var remoteGarden = new GardenCreated(SetIds(new CreateGarden(Guid.NewGuid(), remoteUser.AggregateId), null, remoteUser.AggregateId))
            {
                AggregateVersion = 2,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var remoteAddGarden = new GardenAdded(SetIds(new AddGarden(remoteUser.AggregateId, remoteGarden.EntityId.Value), null, remoteUser.AggregateId))
            {
                AggregateVersion = 3,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var remotePlant = new PlantCreated(SetIds(new CreatePlant(Guid.NewGuid(), "RemoteJare", remoteGarden.EntityId.Value, remoteUser.AggregateId), null, remoteUser.AggregateId))
            {
                AggregateVersion = 1,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var remotePlantProperty = new MarkedPlantPublic(SetIds(new MarkPlantPublic(remotePlant.AggregateId), null, remoteUser.AggregateId))
            {
                AggregateVersion = 2,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };


            var remoteAddPlant = new PlantAdded(SetIds(new AddPlant(remoteGarden.EntityId.Value, remotePlant.AggregateId, remoteUser.AggregateId, "RemoteJare"), null, remoteUser.AggregateId))
            {
                AggregateVersion = 4,
                Created = DateTimeOffset.UtcNow,
                MessageId = Guid.NewGuid()
            };

            var remoteComment = new PlantActionCreated(
                SetIds(
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
                remoteGarden, 
                remoteAddGarden, 
                remotePlant, 
                remoteAddPlant,
                remoteComment,
                remotePlantProperty
            };





            var authResponse = await Transporter.RequestAuthAsync(remoteUser.Username, remoteUser.Password);



            await PushAsAnother(authResponse.AuthToken, remoteEvents);

            remoteEvents.Insert(0, remoteUser);

            return remoteEvents;
        }

        protected async Task PushAsAnother(IAuthToken anotherAuth, IEnumerable<IEvent> events)
        {
            var tmp = HttpClient.AuthToken;
            HttpClient.AuthToken = anotherAuth;

            var pushResp2 = await Transporter.PushAsync(new HttpPushRequest(Get<IJsonFactory>())
            {
                Events = Translator.Out(events).ToArray(),
                ClientDatabaseId = Guid.NewGuid()
            });



            HttpClient.AuthToken = tmp;

        }


        public override async Task ClearDB()
        {
            await Task.Run(async () =>
            {
                await base.ClearDB();
                var db = Kernel.Get<IPersistSyncStreams>();
                db.Purge();
                Kernel.Get<IGSRepository>().ClearCaches();
                Kernel.Get<OptimisticPipelineHook>().Dispose();
                ((FakeUserService)Kernel.Get<IUserService>()).EnsureCurrenUser();

            });
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

