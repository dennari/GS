using Growthstories.Core;
//using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Growthstories.Domain;

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
        bool IsInDesignMode { get; }
        bool CanGoBack { get; }
        bool IsRegistered { get; }
        string AppName { get; }
        IMessageBus Bus { get; }
        IReactiveCommand ShowPopup { get; }
        IReactiveCommand SynchronizeCommand { get; }
        IReactiveCommand UISyncFinished { get; }
        IReactiveCommand DeleteTileCommand { get; }


        IObservable<Tuple<AllSyncResult, GSStatusCode?>> SyncResults { get; }
        IPopupViewModel SyncPopup { get; }
        IUserService Context { get; }
        IAuthUser User { get; }
        IEndpoint Endpoint { get; }
        //IDictionary<IconType, Uri> IconUri { get; }
        //IDictionary<IconType, Uri> BigIconUri { get; }
        IMutableDependencyResolver Resolver { get; }
        //GSApp Model { get; }
        T SetIds<T>(T cmd, Guid? parentId = null, Guid? ancestorId = null) where T : IAggregateCommand;

        IDictionary<Guid, PullStream> SyncStreams { get; }

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
        IObservable<IPlantActionViewModel> CurrentPlantActions(Guid plantId, Guid? PlantActionId = null);
        IObservable<IPlantActionViewModel> FuturePlantActions(Guid plantId, Guid? PlantActionId = null);

        IObservable<IPlantViewModel> CurrentPlants(IAuthUser user, Guid? plantId = null);
        IObservable<IPlantViewModel> FuturePlants(IAuthUser user);

        IObservable<IGardenViewModel> CurrentGardens(IAuthUser user = null);
        IObservable<IGardenViewModel> FutureGardens(IAuthUser user = null);

        IObservable<IScheduleViewModel> FutureSchedules(Guid plantId);

        //IScheduleViewModel ScheduleViewModelFactory(PlantState plantState, ScheduleType scheduleType);
        IAddEditPlantViewModel EditPlantViewModelFactory(IPlantViewModel pvm);
        IYAxisShitViewModel YAxisShitViewModelFactory(IPlantViewModel pvm);

        PageOrientation Orientation { get; }

        bool HasDataConnection { get; }

        IUIPersistence UIPersistence { get; }
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

    public interface ISearchUsersViewModel : IGSRoutableViewModel, IControlsAppBar, IControlsProgressIndicator, IControlsSystemTray
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
        IReactiveCommand DeleteCommand { get; }
        IReactiveCommand DeleteRequestedCommand { get; }
        IReactiveCommand NavigateToEmptyActionCommand { get; }
        IReactiveCommand ShowActionList { get; }

        IReactiveCommand ResetAnimationsCommand { get; }

        //IReactiveCommand ActionTapped { get; }
        //IReactiveCommand AddActionCommand(PlantActionType type);
        IReactiveList<string> Tags { get; }
        Photo Photo { get; }
        int? MissedCount { get; }
        bool HasTile { get; set; }
        bool IsShared { get; set; }
        bool IsFertilizingScheduleEnabled { get; }
        bool IsWateringScheduleEnabled { get; }
        //PlantState State { get; }
        IReadOnlyReactiveList<IPlantActionViewModel> Actions { get; }
        IPlantActionViewModel SelectedItem { get; }
        IScheduleViewModel WateringSchedule { get; }
        IScheduleViewModel FertilizingSchedule { get; }

        IYAxisShitViewModel Chart { get; }

        PlantScheduler WateringScheduler { get; }
        PlantScheduler FertilizingScheduler { get; }

        string TodayWeekDay { get; }
        string TodayDate { get; }

        int PlantIndex { get; set; }

    }



    public sealed class PlantScheduler : ReactiveObject
    {
        public readonly IScheduleViewModel Schedule;


        public const double WINDOW = 0.2;


        public static string IntervalText(long ticks, double Missed)
        {
            string IntervalText;

            var t = new TimeSpan((long)(ticks * Math.Abs(Missed)));
            if (t.TotalWeeks() > 0)
            {
                IntervalText = string.Format("{0:D} weeks, {1:D} days", t.TotalWeeks(), t.DaysAfterWeeks());
            }
            else if (t.Days > 0)
            {
                IntervalText = string.Format("{0:%d} days, {1:D} hours", t, t.Hours);
            }
            else if (t.Hours > 0)
            {
                IntervalText = string.Format("{0:%h} hours", t);
            }
            else
            {
                IntervalText = string.Format("{0:%m} minutes", t);
            }

            return IntervalText;
        }


        public static string NotificationText(TimeSpan Interval, double Missed, ScheduleType Type, String Name)
        {
            var t = new TimeSpan((long)(Interval.Ticks * Math.Abs(Missed)));

            // if now is time to water
            if ((Missed < WINDOW && Missed > -WINDOW))
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return "Time to water " + Name.ToUpper() + "!";

                    case ScheduleType.FERTILIZING:
                        return "Time to nourish " + Name.ToUpper() + "!";
                }
            }

            // if watering is after some time
            if (Missed <= -WINDOW)
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return Name.ToUpper() + " should be watered in " + IntervalText(Interval.Ticks, Missed);

                    case ScheduleType.FERTILIZING:
                        return Name.ToUpper() + " should be nourished in " + IntervalText(Interval.Ticks, Missed);
                }
            }

            // if we are late but not very late
            if (Missed >= WINDOW && Missed < 1)
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return Name + " needs watering";

                    case ScheduleType.FERTILIZING:
                        return Name + " needs nourishing";
                }
            }

            // awfully late
            return string.Format(
                "Last {0} {1} missed for {2}",
                PlantScheduler.GetMissedCount(Missed),
                Type == ScheduleType.WATERING ? "waterings" : "nourishments",
                Name);
        }


        public static string NotificationText(TimeSpan Interval, double Missed, ScheduleType Type)
        {
            var t = new TimeSpan((long)(Interval.Ticks * Math.Abs(Missed)));

            // if now is time to water
            if ((Missed < WINDOW && Missed > -WINDOW))
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return "Time to water!";

                    case ScheduleType.FERTILIZING:
                        return "Time to nourish!";
                }

            }

            // if watering is not late
            if (Missed <= -WINDOW)
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return "watering in " + IntervalText(Interval.Ticks, Missed);

                    case ScheduleType.FERTILIZING:
                        return "nourishing in " + IntervalText(Interval.Ticks, Missed);
                }
            }

            // if we are late but not very late
            if (Missed >= WINDOW && Missed < 1)
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return "needs watering";

                    case ScheduleType.FERTILIZING:
                        return "needs nourishing";
                }
            }

            // awfully late
            return string.Format(
                "Last {0} {1} missed",
                PlantScheduler.GetMissedCount(Missed),
                Type == ScheduleType.WATERING ? "waterings" : "nourishments");
        }


        public PlantScheduler(IScheduleViewModel vm, bool own)
        {
            this.Schedule = vm;
            this._OwnPlant = own;

            this.WhenAnyValue(x => x.LastActionTime)
                .Where(x => x.HasValue)
                .Subscribe(x => this.ComputeNext());

            Action<Task> repeatAction = null;
            repeatAction = _ =>
            {
                // kludge to execute in the main thread
                var kludge = new ReactiveCommand();
                kludge
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                    {
                        this.ComputeNext();
                    });
                kludge.Execute(null);

                // update quickly for debugging
                Task.Delay(1000 * 10).ContinueWith
                    (__ => repeatAction(__));

                // update once in a minute
                //Task.Delay(1000 * 60).ContinueWith
                //    (__ => repeatAction(__));           
            };

            repeatAction(null);
        }


        private bool _OwnPlant;

        private DateTimeOffset? _LastActionTime;
        public DateTimeOffset? LastActionTime
        {
            get
            {
                return _LastActionTime;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _LastActionTime, value);
            }
        }

        public TimeSpan? Interval
        {
            get { return this.Schedule.Interval; }
        }


        /*
         * Calculate time for next scheduled event
         */
        public static DateTimeOffset ComputeNext(DateTimeOffset last, TimeSpan Interval)
        {
            if (Interval == null)
                throw new InvalidOperationException("This schedule is unspecified, so the next occurence cannot be computed.");

            return last + Interval;
        }


        public static double CalculateMissed(DateTimeOffset last, TimeSpan Interval)
        {
            var next = ComputeNext(last, Interval);
            var now = DateTimeOffset.UtcNow;
            var passedSeconds = (long)(now - next).TotalSeconds;

            var num = (double)passedSeconds / Interval.TotalSeconds;

            return num;
        }


        public static int GetMissedCount(double missed)
        {
            if (missed > WINDOW)
            {
                //return (int)Math.Ceiling(missed) + 1;

                return (int)(missed * 1000 + 1) % 100;
            }
            return 0;
        }


        public DateTimeOffset ComputeNext()
        {
            if (!Schedule.Interval.HasValue)
                return DateTimeOffset.MinValue;
            if (!LastActionTime.HasValue)
                return DateTimeOffset.MinValue;

            var last = LastActionTime.Value;
            var ans = ComputeNext(last, (TimeSpan)Schedule.Interval);

            this.Missed = CalculateMissed(last, (TimeSpan)Schedule.Interval);

            string temp = PlantScheduler.NotificationText(Schedule.Interval.Value, this.Missed, Schedule.Type);
            this.MissedText = char.ToUpper(temp[0]) + temp.Substring(1);

            this.WeekDay = ans.ToString("dddd");
            this.Date = ans.ToString("d");
            this.Time = ans.ToString("t");

            return ans;
        }


        // ratio of the time-intervals of how long has passed and how much can pass (the schedule interval)
        // is negative if next scheduled action is in the future
        private double _Missed;
        public double Missed
        {
            get
            {
                return _Missed;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _Missed, value);
                this.raisePropertyChanged("MissedNotification");
                this.raisePropertyChanged("MissedLate");
                this.raisePropertyChanged("MissedLateAndOwn");
                this.raisePropertyChanged("Now");
            }
        }


        public string MissedNotification
        {
            get
            {
                return GetMissedCount(Missed).ToString();
            }
        }

        public bool Now
        {
            get
            {
                return Missed > -WINDOW;
            }
        }

        public bool MissedLate
        {
            get
            {
                return Missed > WINDOW;
            }
        }

        public bool MissedLateAndOwn
        {
            get
            {
                return MissedLate && _OwnPlant;
            }

        }

        private string _MissedText;
        public string MissedText
        {
            get
            {
                return _MissedText;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _MissedText, value);
            }
        }

        private IconType _Icon;
        public IconType Icon
        {
            get
            {
                return _Icon;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Icon, value);
            }
        }

        private string _WeekDay;
        public string WeekDay
        {
            get
            {
                return _WeekDay;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _WeekDay, value);
            }
        }

        private string _Date;
        public string Date
        {
            get
            {
                return _Date;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Date, value);
            }
        }

        private string _Time;
        public string Time
        {
            get
            {
                return _Time;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Time, value);
            }
        }
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


    public interface ISignInRegisterViewModel : IGSRoutableViewModel, IControlsAppBar, IControlsProgressIndicator, IControlsSystemTray, IHasAppBarButtons
    {
        bool IsRegistered { get; }
        bool NavigateBack { get; set; }
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

    }

    //public interface ITimelineActionViewModel : IPlantActionBaseViewModel
    //{

    //}


    public interface IPlantMeasureViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantPhotographViewModel : IPlantActionViewModel
    {
        //IReactiveCommand EditPhotoCommand { get; set; }
        IReactiveCommand PhotoTimelineTap { get; }
        IReactiveCommand PhotoChooserCommand { get; }
    }


    public interface IGSViewModel : IReactiveNotifyPropertyChanged
    {
        IGSAppViewModel App { get; }
    }

    public interface IHasAppBarButtons
    {
        IReadOnlyReactiveList<IButtonViewModel> AppBarButtons { get; }
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
        IReactiveCommand DismissedCommand { get; }
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
        LeftButton,
        RightButton,
        None            // we get this one with back button
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

    public interface IControlsBackButton
    {
        bool CanGoBack { get; }
    }

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
        SIGNUP
    }

    public enum ApplicationBarMode
    {
        DEFAULT,
        MINIMIZED
    }

}
