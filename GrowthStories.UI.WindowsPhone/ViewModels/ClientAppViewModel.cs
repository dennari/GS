
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
using Windows.Devices.Geolocation;


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
                   IScheduleService scheduler,
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
            scheduler,
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


            SignedOut.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                try
                {
                    GSTileUtils.DeleteAllTiles();
                }
                catch { }
            });


            UpdatePhoneLocationServicesEnabled();
        }







        public override async Task<GSLocation> DoGetLocation()
        {
            var pvm = new ProgressPopupViewModel()
            {
                Caption = "Getting location",
                ProgressMessage = "Please wait while Growth Stories figures out your location",
                IsLeftButtonEnabled = false,
            };

            App.ShowPopup.Execute(pvm);

            // make sure the popup is visible at least for a while
            // so user definitely knows that we are getting a location
            await Task.Delay(1000);

            GSLocation location;
            try
            {
                location = await _DoGetLocation();
                App.ShowPopup.Execute(null);
                return location;
            }
            catch (Exception e)
            {
                var popup = new PopupViewModel()
                {
                    Caption = "Could not get location",
                    Message = "We could not figure out your location right know. Please try again later",
                    IsLeftButtonEnabled = true,
                    LeftButtonContent = "OK"
                };
                App.ShowPopup.Execute(popup);
                return null;
            }
        }


        private async Task<GSLocation> _DoGetLocation()
        {
            try
            {
                var gl = new Geolocator();
                gl.DesiredAccuracyInMeters = 50;
                var pos = await gl.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5), timeout: TimeSpan.FromSeconds(15));

                return new GSLocation((float)pos.Coordinate.Latitude, (float)pos.Coordinate.Longitude);
            }

            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    throw new Exception("User has disabled location services");
                }
                throw ex;
            }
        }


        public override void UpdatePhoneLocationServicesEnabled()
        {
            // we need to try to get location to find out whether
            // location services are enabled

            var gl = new Geolocator();
            if (gl.LocationStatus == PositionStatus.Disabled)
            {
                PhoneLocationServicesEnabled = false;
            }
            else
            {
                PhoneLocationServicesEnabled = true;
            }

            /*
            geolocator.LocationStatus
            try
            {
                

                Geolocator geolocator = new Geolocator();
                
                geolocator.DesiredAccuracyInMeters = 5000;
                var pos = await geolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromDays(99), timeout: TimeSpan.FromSeconds(10));
                PhoneLocationServicesEnabled = true;
            
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability 
                    // or the location master switch is off
                    PhoneLocationServicesEnabled = false;
                }
            }
            */
        }

        public bool NavigatingBack { get; set; }


        public override IAddEditPlantViewModel EditPlantViewModelFactory(IPlantViewModel pvm)
        {
            return new ClientAddEditPlantViewModel(this, this.WhenAnyValue(x => x.MyGarden).Where(x => x != null), pvm);
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

