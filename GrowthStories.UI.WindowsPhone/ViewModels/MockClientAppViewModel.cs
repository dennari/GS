
using System;
using System.Threading.Tasks;
using EventStore;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using ReactiveUI.Mobile;


namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public class MockClientAppViewModel : ReactiveObject, IGSAppViewModel, IApplicationRootState
    {


        private IRoutingState _Router;
        public IRoutingState Router
        {
            get
            {
                return _Router;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Router, value);
            }
        }




        public MockClientAppViewModel(
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
                    IMessageBus bus,
                   OptimisticPipelineHook hook
                )
        {
        }



        private IReactiveCommand _PageOrientationChangedCommand;
        public IReactiveCommand PageOrientationChangedCommand
        {
            get
            {
                return _PageOrientationChangedCommand ?? (_PageOrientationChangedCommand = new ReactiveCommand());
            }
        }

        private IReactiveCommand _ShowPopup;
        public IReactiveCommand ShowPopup
        {
            get
            {
                return _ShowPopup ?? (_ShowPopup = new ReactiveCommand());
            }
        }

        private IReactiveCommand _BackKeyPressedCommand;
        public IReactiveCommand BackKeyPressedCommand
        {
            get
            {
                return _BackKeyPressedCommand ?? (_BackKeyPressedCommand = new ReactiveCommand());
            }

        }

        private IReactiveCommand _SetDismissPopupAllowedCommand;
        public IReactiveCommand SetDismissPopupAllowedCommand
        {
            get
            {
                return _SetDismissPopupAllowedCommand ?? (_SetDismissPopupAllowedCommand = new ReactiveCommand());
            }

        }
        public IReactiveCommand _MainWindowLoadedCommand;
        public IReactiveCommand MainWindowLoadedCommand
        {
            get { return _MainWindowLoadedCommand ?? (_MainWindowLoadedCommand = new ReactiveCommand()); }
        }




        public IGardenViewModel MyGarden
        {
            get { return null; }
        }

        public bool IsRegistered
        {
            get { return false; }
        }

        public string AppName
        {
            get { return null; }
        }

        public IMessageBus Bus
        {
            get { return null; }
        }



        public IPopupViewModel SyncPopup
        {
            get { return null; }
        }

        public IAuthUser User
        {
            get { return null; }
        }

        public IMutableDependencyResolver Resolver
        {
            get { return null; }
        }

        public GSApp Model
        {
            get { return null; }
        }

        public T SetIds<T>(T cmd, Guid? parentId = null, Guid? ancestorId = null) where T : IAggregateCommand
        {
            return default(T);
        }

        public System.Collections.Generic.IDictionary<Guid, PullStream> SyncStreams
        {
            get { return null; }
        }

        public bool PhoneLocationServicesEnabled
        {
            get { return false; }
        }

        public Task<IAuthUser> Initialize()
        {
            return null;
        }

        public Task<RegisterResponse> Register(string username, string email, string password)
        {
            return null;
        }

        public Task<SignInResponse> SignIn(string email, string password)
        {
            return null;
        }

        public Task<GSApp> SignOut(bool createUnregUser = true, bool skipLock = false)
        {
            return null;
        }

        public Task<Tuple<AllSyncResult, GSStatusCode?>> Synchronize()
        {
            return null;
        }

        public Task<IGSAggregate> HandleCommand(IAggregateCommand x)
        {
            return null;
        }

        public Task<IGSAggregate> HandleCommand(MultiCommand x)
        {
            return null;
        }

        public IMainViewModel CreateMainViewModel()
        {
            return new MainViewModel(
                () => (IGardenViewModel)null,
                () => (INotificationsViewModel)null,
                () => (FriendsViewModel)null,
                this
                );
        }


        public IPlantActionViewModel PlantActionViewModelFactory(PlantActionType type, PlantActionState state = null)
        {
            return null;
        }

        public IObservable<IPlantActionViewModel> CurrentPlantActions(Guid PlantId, PlantActionType? type = null, int? limit = null, bool? isOrderAsc = null)
        {
            return null;
        }

        public IObservable<IPlantActionViewModel> FuturePlantActions(Guid plantId, Guid? PlantActionId = null)
        {
            return null;
        }

        public IObservable<IPlantViewModel> CurrentPlants(Guid? userId = null, Guid? plantId = null)
        {
            return null;
        }

        public IPlantViewModel GetSinglePlant(Guid plantId)
        {
            return null;
        }

        public IObservable<IPlantViewModel> FuturePlants(Guid userId)
        {
            return null;
        }

        public IObservable<IGardenViewModel> CurrentPYFs(Guid? userId = null)
        {
            return null;
        }

        public IObservable<IGardenViewModel> FuturePYFs(Guid? userId = null)
        {
            return null;
        }

        public IObservable<IScheduleViewModel> FutureSchedules(Guid plantId)
        {
            return null;
        }

        public IAddEditPlantViewModel EditPlantViewModelFactory(IPlantViewModel pvm)
        {
            return null;
        }

        public IYAxisShitViewModel YAxisShitViewModelFactory(IPlantViewModel pvm)
        {
            return null;
        }

        public PageOrientation Orientation
        {
            get { return PageOrientation.Portrait; }
        }

        public bool HasDataConnection
        {
            get { return true; }
        }

        public bool EnsureDataConnection()
        {
            return true;
        }

        public Task<GSLocation> GetLocation()
        {
            return null;
        }

        public bool GSLocationServicesEnabled
        {
            get { return true; }
        }

        public void UpdatePhoneLocationServicesEnabled()
        {

        }

        public GSLocation LastLocation
        {
            get { return null; }
        }

        public void NotifyImageDownloadFailed()
        {
        }

        public bool NotifiedOnBadConnection
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public bool RegisterCancelRequested
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public bool SignInCancelRequested
        {
            get
            {
                return false;
            }
            set
            {
            }
        }



        public string UserEmail
        {
            get { return null; }
        }

        public ISearchUsersViewModel SearchUsersViewModelFactory(IFriendsViewModel friendsVM)
        {
            return null;
        }



        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get { return null; }
        }

        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get { return null; }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.MINIMIZED; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }

        public string UrlPath
        {
            get { return null; }
        }

        public string UrlPathSegment
        {
            get { return null; }
        }

        public IScreen HostScreen
        {
            get { return this; }
        }


        public bool NavigatingBack
        {
            get
            {
                return false;
            }
            set
            {

            }
        }



        public void HandleApplicationActivated()
        {
        }





        public IGSViewModel DefaultVM
        {
            get { return null; }
        }


        public IPlantViewModel SelectedPlant
        {
            get
            {
                return null;
            }
            set
            {

            }
        }
    }



}

