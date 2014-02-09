
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
using Microsoft.Phone.Info;


namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public class MockClientAppViewModel : ReactiveObject, IGSAppViewModel, IApplicationRootState
    {


        private IRoutingState _Router;
        public IRoutingState Router { get { return _Router ?? (_Router = new RoutingState()); } }



        public MockClientAppViewModel()
        {

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

        public IReactiveCommand ShowPopup
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
            return null;
        }

        public IReactiveCommand BackKeyPressedCommand
        {
            get { return null; }
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

        public IReactiveCommand SetDismissPopupAllowedCommand
        {
            get { return null; }
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
    }



}

