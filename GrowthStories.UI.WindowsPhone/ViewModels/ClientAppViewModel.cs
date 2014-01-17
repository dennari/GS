
using System;
using System.Reactive.Linq;
using EventStore;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using GrowthStories.UI.WindowsPhone.BA;
using ReactiveUI;
using ReactiveUI.Mobile;


namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public sealed class ClientAppViewModel : AppViewModel, IApplicationRootState
    {



        private Microsoft.Phone.Controls.SupportedPageOrientation _ClientSupportedOrientations
            = Microsoft.Phone.Controls.SupportedPageOrientation.Portrait;
        private readonly OptimisticPipelineHook Hook;
        private readonly IPersistSyncStreams Store;

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


        public ClientAppViewModel(
                   IMutableDependencyResolver resolver,
                   IUserService context,
                   IDispatchCommands handler,
                   IGSRepository repository,
                   ITransportEvents transporter,
                   IUIPersistence uiPersistence,
                   IPersistSyncStreams store,
                   IIAPService iiapService,
                   IRequestFactory requestFactory,
                   IRoutingState router,
                    IMessageBus bus,
                   OptimisticPipelineHook hook
                )
            : base(
            resolver,
            context,
            handler,
            repository,
            transporter,
            uiPersistence,
            iiapService,
            requestFactory,
            router,
            bus
            )
        {
            this.Store = store;
            this.Hook = hook;

            Initialize();

            this.WhenAny(x => x.SupportedOrientations, x => x.GetValue()).Subscribe(x =>
            {
                try
                {
                    this.ClientSupportedOrientations = (Microsoft.Phone.Controls.SupportedPageOrientation)x;

                }
                catch { }
            });

            MyGardenCreatedCommand.Subscribe(x =>
            {
                try
                {
                    //GSTileUtils.SubscribeForTileUpdates(x as IGardenViewModel);
                    //StartScheduleUpdater(x as IGardenViewModel);
                }
                catch { }
            });

            SignedOut.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                try
                {
                    GSTileUtils.DeleteAllTiles();
                }
                catch { }
            });

            DeleteTileCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
            {
                GSTileUtils.DeleteTile((IPlantViewModel)x);
            });

            //IAPCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(async x => 
            //{
            //    var ret = await GSIAP.ShopForBasicProduct();
            //    AfterIAPCommand.Execute(ret);
            //});

            //GSIAP.PossiblySetupMockIAP();
        }




        //protected void StartScheduleUpdater(IGardenViewModel gvm)
        //{
        //    Action<Task> repeatAction = null;
        //    repeatAction = _ =>
        //    {
        //        // kludge to execute in the main thread
        //        var kludge = new ReactiveCommand();
        //        kludge
        //            .ObserveOn(RxApp.MainThreadScheduler)
        //            .Subscribe(x =>
        //            {
        //                RecalculateSchedules(gvm);
        //            });
        //        kludge.Execute(null);

        //        // update quickly for debugging
        //        Task.Delay(1000 * 20).ContinueWith
        //            (__ => repeatAction(__));

        //        // update once in a minute
        //        //Task.Delay(1000 * 60).ContinueWith
        //        //    (__ => repeatAction(__));           
        //    };
        //    repeatAction(null);
        //}


        //protected void RecalculateSchedules(IGardenViewModel gvm)
        //{

        //    foreach (var plant in gvm.Plants)
        //    {
        //        if (plant.WateringScheduler != null)
        //        {
        //            plant.WateringScheduler.ComputeNext();
        //        }
        //        if (plant.FertilizingScheduler != null)
        //        {
        //            plant.FertilizingScheduler.ComputeNext();
        //        }
        //    }
        //}




        public bool NavigatingBack { get; set; }


        public override IAddEditPlantViewModel EditPlantViewModelFactory(IPlantViewModel pvm)
        {
            return new ClientAddEditPlantViewModel(this, pvm);
        }

        public override IYAxisShitViewModel YAxisShitViewModelFactory(IPlantViewModel pvm)
        {
            return new ClientYAxisShitViewModel(pvm, this);
        }

        public override IPlantActionViewModel PlantActionViewModelFactory(PlantActionType type, PlantActionState state = null)
        {
            if (type == PlantActionType.PHOTOGRAPHED)
                return new ClientPlantPhotographViewModel(this, state);
            else
                return base.PlantActionViewModelFactory(type, state);
        }

        protected override IPlantViewModel PlantViewModelFactory(IObservable<Tuple<PlantState, ScheduleState, ScheduleState>> stateObservable)
        {
            return new ClientPlantViewModel(
               stateObservable,
                pvm => new TileHelper(pvm, this.User),
                this
             );
        }


        protected override void ClearDB()
        {
            // clear isolated storage containing tile update infos
            GSTileUtils.ClearAllTileUpdateInfos();


            Store.ReInitialize();
            UIPersistence.ReInitialize();
            Repository.ClearCaches();
            Hook.Dispose();



        }


    }


}

