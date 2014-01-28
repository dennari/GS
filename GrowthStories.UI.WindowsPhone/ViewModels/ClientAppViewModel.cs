
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
using Windows.Foundation;



namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public class ClientAppViewModel : AppViewModel, IApplicationRootState
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
                     ISynchronizer synchronizer,
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
            synchronizer,
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


        // ( could also be in AppViewModel )
        protected override async Task<GSLocation> DoGetLocation()
        {
            var pvm = new ProgressPopupViewModel()
            {
                Caption = "Getting location",
                ProgressMessage = "Growth Stories is determining your location.",
                //IsLeftButtonEnabled = true,
                //LeftButtonContent = "Do in background",
            };

            ShowPopup.Execute(pvm);

            // make sure the popup is visible at least for a while
            // so user definitely knows that we are getting a location
            await Task.Delay(1000);

            GSLocation location = null;
            try
            {
                location = await _DoGetLocation();
                ShowPopup.Execute(null);
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
                ShowPopup.Execute(null);
                ShowPopup.Execute(popup);
                return null;
            }
        }


        public override void HandleApplicationActivated()
        {
            base.HandleApplicationActivated();
            UpdateHasTiles();
        }


        private async Task<GSLocation> _DoGetLocation()
        {

            try
            {
                var gl = new Geolocator();
                gl.DesiredAccuracyInMeters = 50;
                this.Log().Info("getGeoPosition");

                var age = TimeSpan.FromMinutes(5);
                var timeout = TimeSpan.FromSeconds(10);

                var posTask = gl.GetGeopositionAsync(maximumAge: age, timeout: timeout);
                Geoposition pos;
                try
                {
                    pos = await posTask;
                    this.Log().Info("getGeoPosition returned {0}", pos.ToString());
                    return new GSLocation((float)pos.Coordinate.Latitude, (float)pos.Coordinate.Longitude);
                }
                finally
                {
                    // http://stackoverflow.com/questions/18713557/getgeopositionasync-never-finishes-works-pefect-the-second-time-it-is-called
                    if (posTask != null)
                    {
                        posTask.Cancel();
                    }
                }
            }

            catch (Exception ex)
            {
                this.Log().Info("getGeoPositionAsync throwing exception {0}", ex.Message);

                if ((uint)ex.HResult == 0x80004004)
                {
                    throw new Exception("User has disabled location services");
                }
                throw ex;
            }

        }

        public void UpdateHasTiles()
        {
            if (MyGarden == null)
                return;
            foreach (var p in MyGarden.Plants)
            {
                var gp = p as ClientPlantViewModel;
                if (gp != null && gp.TileHelper != null)
                {
                    gp.TileHelper.UpdateHasTile();
                }
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

            // 
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

