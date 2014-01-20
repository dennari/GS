using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Growthstories.Core;
//using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Growthstories.UI.Services;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{
    public interface IAddEditPlantViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsAppBar
    {
        IReactiveCommand ChooseProfilePictureCommand { get; }
        IReactiveCommand ChooseWateringSchedule { get; }
        IReactiveCommand AddTag { get; }
        IReactiveCommand RemoveTag { get; }
        IReactiveCommand ChooseFertilizingSchedule { get; }
        IReactiveList<string> Tags { get; }
        IScheduleViewModel WateringSchedule { get; }
        IScheduleViewModel FertilizingSchedule { get; }
        string Title { get; }
    }

    public interface IFriendsViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsAppBar, ICanSelect
    {
        IReadOnlyReactiveList<IGardenViewModel> Friends { get; }
        IGardenViewModel SelectedFriend { get; }
    }

    //public enum ScheduleType
    //{
    //    WATERING,
    //    FERTILIZING
    //}

    public interface IUserViewModel
    {

    }

    public enum RegisterResponse
    {
        alreadyRegistered,
        emailInUse,
        success,
        tryagain,
        connectionerror,
        usernameInUse,
        almostSuccess     // registered but sync failed after that
    }

    public enum SignInResponse
    {
        invalidEmail,
        invalidPassword,
        tryagain,
        success,
        connectionerror
    }


    public interface IGSAppViewModel : IGSRoutableViewModel, IScreen, IHasAppBarButtons, IHasMenuItems, IControlsAppBar
    {
        //bool CanGoBack { get; }
        bool IsRegistered { get; }
        string AppName { get; }
        IMessageBus Bus { get; }
        IReactiveCommand ShowPopup { get; }
        IReactiveCommand SynchronizeCommand { get; }
        IReactiveCommand UISyncFinished { get; }
        IObservable<Tuple<AllSyncResult, GSStatusCode?>> SyncResults { get; }
        IPopupViewModel SyncPopup { get; }
        IAuthUser User { get; }
        //IDictionary<IconType, Uri> IconUri { get; }
        //IDictionary<IconType, Uri> BigIconUri { get; }
        IMutableDependencyResolver Resolver { get; }
        //GSApp Model { get; }
        T SetIds<T>(T cmd, Guid? parentId = null, Guid? ancestorId = null) where T : IAggregateCommand;

        IDictionary<Guid, PullStream> SyncStreams { get; }

        bool PhoneLocationServicesEnabled { get; }

        Task<IAuthUser> Initialize();
        Task<RegisterResponse> Register(string username, string email, string password);
        Task<SignInResponse> SignIn(string email, string password);
        Task<GSApp> SignOut(bool createUnregUser = true);
        Task<ISyncInstance> Synchronize();
        Task<ISyncInstance> Push();
        Task<IGSAggregate> HandleCommand(IAggregateCommand x);
        Task<IGSAggregate> HandleCommand(MultiCommand x);
        //IObservable<IUserViewModel> Users();
        //IObservable<IGardenViewModel> Gardens { get; }
        //IObservable<IPlantViewModel> Plants { get; }
        //IObservable<IPlantActionViewModel> PlantActions(Guid guid);

        IReactiveCommand BackKeyPressedCommand { get; }
        IPlantActionViewModel PlantActionViewModelFactory(PlantActionType type, PlantActionState state = null);
        IObservable<IPlantActionViewModel> CurrentPlantActions(
            Guid PlantId,
            PlantActionType? type = null,
            int? limit = null,
            bool? isOrderAsc = null
            );
        IObservable<IPlantActionViewModel> FuturePlantActions(Guid plantId, Guid? PlantActionId = null);

        IObservable<IPlantViewModel> CurrentPlants(Guid? userId = null, Guid? plantId = null);
        IPlantViewModel GetSinglePlant(Guid plantId);
        IObservable<IPlantViewModel> FuturePlants(Guid userId);

        IObservable<IGardenViewModel> CurrentGardens(Guid? userId = null);
        IObservable<IGardenViewModel> FutureGardens(Guid? userId = null);

        IObservable<IScheduleViewModel> FutureSchedules(Guid plantId);

        //IScheduleViewModel ScheduleViewModelFactory(PlantState plantState, ScheduleType scheduleType);
        IAddEditPlantViewModel EditPlantViewModelFactory(IPlantViewModel pvm);
        IYAxisShitViewModel YAxisShitViewModelFactory(IPlantViewModel pvm);

        PageOrientation Orientation { get; }

        bool HasDataConnection { get; }

        bool EnsureDataConnection();
        //IUIPersistence UIPersistence { get; }

        //Task PossiblyAutoSync();
        void PossiblyAutoSync();

        Task<GSLocation> GetLocation();

        bool GSLocationServicesEnabled { get; }

        void UpdatePhoneLocationServicesEnabled();

        GSLocation LastLocation { get; }

        IEnumerable<Guid> GetCurrentFollowers(Guid userId);

        bool NotifiedOnBadConnection { get; set; }
    }


    public interface IGardenViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar, IHasMenuItems
    {
        Guid Id { get; }
        Guid UserId { get; }
        IReactiveCommand SelectedItemsChanged { get; }
        IAuthUser User { get; }
        IReadOnlyReactiveList<IPlantViewModel> Plants { get; }
        string Username { get; }

        IReactiveCommand ShowDetailsCommand { get; }
        IGardenPivotViewModel PivotVM { get; }
    }


    public interface IGardenPivotViewModel : IGardenViewModel, IMultipageViewModel, IControlsPageOrientation
    {
        IPlantViewModel SelectedPlant { get; }
        IRoutableViewModel InnerViewModel { get; }
        //Type NavigateInterface { get; }
    }


    public interface INotificationsViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }

    public interface ISearchUsersViewModel : IGSRoutableViewModel, IControlsAppBar, IControlsProgressIndicator, IControlsSystemTray, IRequiresNetworkConnection
    {
        IReadOnlyReactiveList<RemoteUser> List { get; }
        IReactiveCommand SearchCommand { get; }
        IReactiveCommand UserSelectedCommand { get; }
        string Search { get; set; }
    }


    public interface IPlantActionListViewModel : IGSRoutableViewModel, IControlsAppBar
    {
        IPlantViewModel Plant { get; set; }
        IReactiveCommand NavigateToSelected { get; }
    }


    public interface IMultipageViewModel : IGSRoutableViewModel
    {
        IGSViewModel SelectedPage { get; set; }
        object SelectedItem { get; set; } // for compability
        IReadOnlyReactiveList<IGSViewModel> Items { get; }
        IReactiveCommand PageChangedCommand { get; }
    }


    public interface IMainViewModel : IMultipageViewModel, IControlsSystemTray, IControlsProgressIndicator
    {
        IGardenViewModel GardenVM { get; }
        INotificationsViewModel NotificationsVM { get; }
        FriendsViewModel FriendsVM { get; }
    }

    public interface ICommandViewModel : IGSRoutableViewModel, IHasAppBarButtons
    {
        IReactiveCommand AddCommand { get; }
        IObservable<bool> CanExecute { get; }
        string TopTitle { get; }
        string Title { get; }

    }

    public interface IPlantViewModel : IGSRoutableViewModel, IHasAppBarButtons, IHasMenuItems, IControlsAppBar, IControlsPageOrientation
    {
        Guid Id { get; }
        Guid UserId { get; }
        string Name { get; }
        string Species { get; }
        IReactiveCommand PinCommand { get; }
        IReactiveCommand ShareCommand { get; }
        IReactiveCommand ScrollCommand { get; }
        IReactiveCommand WateringCommand { get; }
        IObservable<IPlantViewModel> DeleteObservable { get; }
        IReactiveCommand DeleteCommand { get; }
        //IReactiveCommand DeleteRequestedCommand { get; }
        IReactiveCommand NavigateToEmptyActionCommand { get; }
        IReactiveCommand ShowActionList { get; }
        IReactiveCommand ShowDetailsCommand { get; }
        IReactiveCommand ResetAnimationsCommand { get; }

        //IReactiveCommand ActionTapped { get; }
        //IReactiveCommand AddActionCommand(PlantActionType type);
        IReactiveList<string> Tags { get; }
        Photo Photo { get; }
        int? MissedCount { get; }
        bool HasTile { get; }
        bool IsShared { get; set; }

        bool IsFertilizingScheduleEnabled { get; }
        bool IsWateringScheduleEnabled { get; }
        //PlantState State { get; }
        IReadOnlyReactiveList<IPlantActionViewModel> Actions { get; }
        IScheduleViewModel WateringSchedule { get; set; }
        IScheduleViewModel FertilizingSchedule { get; set; }

        IYAxisShitViewModel Chart { get; }

        PlantScheduler WateringScheduler { get; }
        PlantScheduler FertilizingScheduler { get; }

        string TodayWeekDay { get; }
        string TodayDate { get; }

        int PlantIndex { get; set; }

        bool HasWriteAccess { get; }

        GSLocation Location { get; }

        bool Loaded { get; }

        bool ShowPlaceHolder { get; }

        void NotifyImageDownloadFailed();

    }


    public interface IPlantSingularViewModel : IGSRoutableViewModel, IMultipageViewModel, IHasAppBarButtons, IHasMenuItems, IControlsAppBar, IControlsPageOrientation
    {
        IPlantViewModel Plant { get; }
    }



    public interface IPhotoListViewModel : IGSRoutableViewModel, IControlsAppBar, IControlsPageOrientation
    {
        IList<IPlantPhotographViewModel> Photos { get; }

        IPlantPhotographViewModel Selected { get; }

    }



    public interface ITileHelper
    {
        bool CreateOrUpdateTile();
        bool DeleteTile();

        bool HasTile { get; }

    }





    public interface IScheduleViewModel : IGSRoutableViewModel
    {
        //bool IsEnabled { get; set; }
        Guid? Id { get; }
        TimeSpan? Interval { get; set; }
        IReactiveList<Tuple<IPlantViewModel, IScheduleViewModel>> OtherSchedules { get; set; }
        bool HasChanged { get; }
        //string Value { get; }
        //object SelectedValueType { get; set; }
        //IntervalValue ValueType { get; }
        string IntervalLabel { get; }
        string ScheduleTypeLabel { get; }
        //string IntervalLabel { get; }
        ScheduleType Type { get; }

        //DateTimeOffset ComputeNext(DateTimeOffset last);
        Task<Schedule> Create();

    }


    public interface ISettingsViewModel : IGSRoutableViewModel, IControlsAppBar, IControlsSystemTray, IHasAppBarButtons
    {

        IButtonViewModel LocationServices { get; }
        IButtonViewModel SharedByDefault { get; }
        IReactiveCommand NavigateToAbout { get; }
        //IReactiveCommand WarnCommand { get; }
        //IReactiveCommand WarningDismissedCommand { get; }
        IReactiveCommand SignOutCommand { get; }
        //bool DialogIsOn { get; set; }
    }

    public interface IAboutViewModel : IGSRoutableViewModel, IControlsAppBar, IControlsSystemTray
    {


    }


    public interface ISignInRegisterViewModel : IGSRoutableViewModel, IControlsAppBar, IControlsProgressIndicator, IControlsSystemTray, IHasAppBarButtons, IRequiresNetworkConnection
    {
        bool IsRegistered { get; }
        //bool NavigateBack { get; set; }
        IObservable<Tuple<bool, RegisterResponse, SignInResponse>> Response { get; }
        IReactiveCommand OKCommand { get; }
        string Username { get; set; }
        string Password { get; set; }
        string PasswordConfirmation { get; set; }
        string Email { get; set; }
        string Message { get; }
        //string Username { get; set; }

        string UsernameComplaint { get; }
        bool UsernameTouched { get; set; }

        string EmailComplaint { get; }
        bool EmailTouched { get; set; }

        string PasswordComplaint { get; }
        bool PasswordTouched { get; set; }

        string PasswordConfirmationComplaint { get; }
        bool PasswordConfirmationTouched { get; set; }

        bool SignInMode { get; }

    }


    public interface IYAxisShitViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsPageOrientation, IControlsAppBar
    {
        //sIDictionary<MeasurementType, ISeries> Series { get; }
        //IDictionary<MeasurementType, object> TelerikSeries { get; }
        IReactiveCommand ToggleSeries { get; }
        //IReactiveCommand ToggleTelerikSeries { get; }
        double Minimum { get; set; }
        double Maximum { get; set; }
        double YAxisStep { get; set; }
        double YAxisLabelStep { get; set; }
        double XAxisLabelStep { get; set; }
        string LineColor { get; set; }
        IReadOnlyReactiveList<IPlantMeasureViewModel> Series { get; set; }

    }


    public interface IPlantActionBaseViewModel : IGSViewModel
    {

        string WeekDay { get; }
        string Date { get; }
        string Time { get; }
        string Note { get; }
        string Label { get; }
        PlantActionType ActionType { get; }
        IconType Icon { get; }
        Guid PlantActionId { get; }
        DateTimeOffset Created { get; }
        MeasurementType MeasurementType { get; }
        double? Value { get; }
        Photo Photo { get; }

        PlantActionState State { get; }

        //void SetProperty(PlantActionPropertySet prop);
    }



    public interface IPlantActionViewModel : IPlantActionBaseViewModel, ICommandViewModel
    {
        IReactiveCommand OpenZoomView { get; }
        Guid? PlantId { get; set; }
        Guid? UserId { get; set; }
        IReactiveCommand EditCommand { get; }
        IReactiveCommand DeleteCommand { get; }
        string TimelineFirstLine { get; }
        string TimelineSecondLine { get; }
        int ActionIndex { get; set; }
        bool OwnAction { get; set; }

        IObservable<IPlantActionViewModel> AsyncAddObservable { get; }


    }

    //public interface ITimelineActionViewModel : IPlantActionBaseViewModel
    //{

    //}


    public interface IPlantMeasureViewModel : IPlantActionViewModel
    {

        // References previous measurement of same type
        //
        IPlantMeasureViewModel PreviousMeasurement { get; set; }
        IReactiveDerivedList<IPlantMeasureViewModel> MeasurementActions { get; set; }

    }

    public interface IPlantPhotographViewModel : IPlantActionViewModel
    {
        //IReactiveCommand EditPhotoCommand { get; set; }
        IReactiveCommand PhotoTimelineTap { get; }
        IReactiveCommand PhotoChooserCommand { get; }
   
        bool IsProfilePhoto {get; set;}
    }


    public interface IGSViewModel : IReactiveNotifyPropertyChanged
    {
        //IGSAppViewModel App { get; }
    }

    public interface IHasAppBarButtons
    {
        IReadOnlyReactiveList<IButtonViewModel> AppBarButtons { get; }
    }

    public interface IRequiresNetworkConnection
    {
        IPopupViewModel NoConnectionAlert { get; }
    }

    public interface IHasMenuItems
    {
        IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems { get; }
    }

    public interface ICanSelect
    {
        object SelectedItem { get; set; }
    }

    public interface IMenuItemViewModel
    {
        IReactiveCommand Command { get; }
        object CommandParameter { get; }
        string Text { get; }
        bool IsEnabled { get; set; }
    }

    public interface IButtonViewModel : IMenuItemViewModel
    {
        IconType IconType { get; }
    }

    public interface IPopupViewModel
    {
        //IReactiveCommand DismissedCommand { get; }

        void Dismiss(PopupResult x);

        IObservable<PopupResult> DismissedObservable { get; }
        IObservable<PopupResult> AcceptedObservable { get; }


        string Caption { get; }
        string Message { get; }
        string LeftButtonContent { get; }
        string RightButtonContent { get; }
        bool IsLeftButtonEnabled { get; }
        bool IsRightButtonEnabled { get; }
        bool IsFullScreen { get; }
        PopupType Type { get; }
        string ProgressMessage { get; }
    }

    public enum PopupType
    {
        BASIC,
        PROGRESS,
    }

    public enum PopupResult
    {
        Null, // this is meant to signify a placeholder value, i.e. no no selection has yet been made
        LeftButton,
        RightButton,
        None, // we get this one with back button
    }

    public interface IGSRoutableViewModel : IRoutableViewModel, IGSViewModel
    {
        string UrlPath { get; }
    }

    public enum SupportedPageOrientation
    {
        // Summary:
        //     Portrait orientation.
        Portrait = 1,
        //
        // Summary:
        //     Landscape orientation. Landscape supports both left and right views, but
        //     there is no way programmatically to specify one or the other.
        Landscape = 2,
        //
        // Summary:
        //     Landscape or portrait orientation.
        PortraitOrLandscape = 3,
    }

    public enum PageOrientation
    {
        // Summary:
        //     No orientation is specified.
        None = 0,
        //
        // Summary:
        //     Portrait orientation.
        Portrait = 1,
        //
        // Summary:
        //     Landscape orientation.
        Landscape = 2,
        //
        // Summary:
        //     Portrait orientation.
        PortraitUp = 5,
        //
        // Summary:
        //     Portrait orientation. This orientation is never used.
        PortraitDown = 9,
        //
        // Summary:
        //     Landscape orientation with the top of the page rotated to the left.
        LandscapeLeft = 18,
        //
        // Summary:
        //     Landscape orientation with the top of the page rotated to the right.
        LandscapeRight = 34,
    }

    public interface IControlsPageOrientation
    {
        SupportedPageOrientation SupportedOrientations { get; }
        //ReactiveCommand PageOrientationChangedCommand { get; }
    }

    public interface IControlsAppBar
    {
        ApplicationBarMode AppBarMode { get; }
        bool AppBarIsVisible { get; }
    }

    //public interface IControlsBackButton
    //{
    //    bool CanGoBack { get; }
    //}

    public interface IControlsSystemTray
    {
        bool SystemTrayIsVisible { get; }
    }

    public interface IControlsProgressIndicator
    {
        bool ProgressIndicatorIsVisible { get; }
    }



    public static class ViewModelMixins
    {

    }

    public enum IconType
    {
        ADD,
        CHECK,
        CANCEL,
        DELETE,
        CHECK_LIST,
        WATER,
        FERTILIZE,
        PHOTO,
        NOTE,
        MEASURE,
        NOURISH,
        CHANGESOIL,
        SHARE,
        BLOOMING,
        DECEASED,
        ILLUMINANCE,
        LENGTH,
        MISTING,
        PH,
        POLLINATION,
        SPROUTING,
        PRUNING,
        HARVESTING,
        CO2,
        AIRHUMIDITY,
        PH2,
        SETTINGS,
        SIGNOUT,
        SIGNIN,
        SIGNUP,
        ARROW_UP,
        ARROW_DOWN,
        ARROW_RIGHT,
    }

    public enum ApplicationBarMode
    {
        DEFAULT,
        MINIMIZED
    }

}
