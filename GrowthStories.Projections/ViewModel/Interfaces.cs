using Growthstories.Core;
//using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{
    public interface IAddPlantViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsAppBar
    {
        IReactiveCommand ChooseProfilePictureCommand { get; }
        IReactiveCommand ChooseWateringSchedule { get; }
        IReactiveCommand AddTag { get; }
        IReactiveCommand RemoveTag { get; }
        IReactiveCommand ChooseFertilizingSchedule { get; }
        IReactiveList<string> Tags { get; }
        IScheduleViewModel WateringSchedule { get; }
        IScheduleViewModel FertilizingSchedule { get; }

    }

    public interface IFriendsViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsAppBar
    {
        IReactiveCommand FriendTapped { get; }
        IReadOnlyReactiveList<IGardenViewModel> Friends { get; }
        IGardenViewModel SelectedItem { get; }
    }

    //public enum ScheduleType
    //{
    //    WATERING,
    //    FERTILIZING
    //}

    public interface IUserViewModel
    {

    }

    public interface IGSAppViewModel : IGSRoutableViewModel, IScreen, IHasAppBarButtons, IHasMenuItems, IControlsAppBar
    {
        bool IsInDesignMode { get; }
        string AppName { get; }
        IMessageBus Bus { get; }

        IUserService Context { get; }
        IAuthUser User { get; }
        //IDictionary<IconType, Uri> IconUri { get; }
        //IDictionary<IconType, Uri> BigIconUri { get; }
        IMutableDependencyResolver Resolver { get; }
        //GSApp Model { get; }
        T SetIds<T>(T cmd, Guid? parentId = null, Guid? ancestorId = null) where T : IAggregateCommand;

        Task<IAuthUser> Initialize();
        Task<IList<ISyncInstance>> Synchronize();
        Task<IGSAggregate> HandleCommand(IAggregateCommand x);
        Task<IGSAggregate> HandleCommand(MultiCommand x);
        //IObservable<IUserViewModel> Users();
        //IObservable<IGardenViewModel> Gardens { get; }
        //IObservable<IPlantViewModel> Plants { get; }
        //IObservable<IPlantActionViewModel> PlantActions(Guid guid);


        //IPlantActionViewModel PlantActionViewModelFactory<T>(PlantActionState state = null) where T : IPlantActionViewModel;
        IObservable<IPlantActionViewModel> CurrentPlantActions(PlantState state, Guid? PlantActionId = null);
        IObservable<IPlantActionViewModel> FuturePlantActions(PlantState state, Guid? PlantActionId = null);

        IObservable<IPlantViewModel> CurrentPlants(IAuthUser user);
        IObservable<IPlantViewModel> FuturePlants(IAuthUser user);

        IObservable<IGardenViewModel> CurrentGardens(IAuthUser user = null);
        IObservable<IGardenViewModel> FutureGardens(IAuthUser user = null);

        IObservable<IScheduleViewModel> FutureSchedules(Guid plantId);

        IScheduleViewModel ScheduleViewModelFactory(PlantState plantState, ScheduleType scheduleType);
        IAddPlantViewModel AddPlantViewModelFactory(PlantState state);
        IYAxisShitViewModel YAxisShitViewModelFactory(IPlantViewModel pvm);

        PageOrientation Orientation { get; }
        //Task AddTestData();
        //Task ClearDB();


        //IGardenViewModel GardenFactory(Guid guid);



        //IPlantWaterViewModel BuildNextWatering(IPlantActionViewModel a);
        //IPlantFertilizeViewModel BuildNextNourishing(IPlantActionViewModel a);

    }

    public interface IGardenViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar, IHasMenuItems, IControlsPageOrientation
    {
        Guid Id { get; }
        //GardenState State { get; }
        IPlantViewModel SelectedItem { get; }
        IReactiveCommand SelectedItemsChanged { get; }

        IAuthUser User { get; }
        IReadOnlyReactiveList<IPlantViewModel> Plants { get; }
        string Username { get; }
    }

    public interface INotificationsViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }

    public interface ISearchUsersViewModel : IControlsAppBar, IControlsProgressIndicator, IControlsSystemTray
    {
        IReadOnlyReactiveList<RemoteUser> List { get; }
        IReactiveCommand SearchCommand { get; }
        IReactiveCommand UserSelectedCommand { get; }
    }


    public interface IMultipageViewModel : IGSRoutableViewModel
    {
        IGSViewModel SelectedItem { get; set; }
        IReadOnlyReactiveList<IGSViewModel> Items { get; }
        IReactiveCommand PageChangedCommand { get; }
    }


    public interface IMainViewModel : IGSRoutableViewModel, IControlsSystemTray, IControlsProgressIndicator
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
        IReactiveCommand ScrollCommand { get; }
        IReactiveCommand ActionTapped { get; }
        Photo Photo { get; }
        //PlantState State { get; }
        IReadOnlyReactiveList<IPlantActionViewModel> Actions { get; }
        IPlantActionViewModel SelectedItem { get; }
        IScheduleViewModel WateringSchedule { get; }
        IScheduleViewModel FertilizingSchedule { get; }

        PlantScheduler WateringScheduler { get; }
        PlantScheduler FertilizingScheduler { get; }


        string TodayWeekDay { get; }
        string TodayDate { get; }


    }



    public sealed class PlantScheduler
    {
        private IScheduleViewModel Schedule;

        public PlantScheduler(IScheduleViewModel vm)
        {
            this.Schedule = vm;
        }

        public DateTimeOffset ComputeNext(DateTimeOffset last)
        {
            var ans = Schedule.ComputeNext(last);

            var now = DateTimeOffset.UtcNow;

            var passedSeconds = (long)(now - last).TotalSeconds;

            var num = (int)(passedSeconds / Schedule.Interval);
            if (num > 0)
            {
                this.Missed = num;
                if (num == 1)
                    this.MissedText = string.Format("Last {0} missed.", Schedule.Type == ScheduleType.WATERING ? "watering" : "nourishment");
                else
                    this.MissedText = string.Format("Last {0} {1} missed.", num, Schedule.Type == ScheduleType.WATERING ? "waterings" : "nourishments");

            }
            else
            {
                this.Missed = null;
                this.MissedText = null;
            }

            this.WeekDay = ans.ToString("dddd");
            this.Date = ans.ToString("d");
            this.Time = ans.ToString("t");

            return ans;
        }


        public int? Missed { get; private set; }
        public string MissedText { get; private set; }
        public IconType IconType { get; set; }

        public string WeekDay { get; private set; }
        public string Date { get; private set; }
        public string Time { get; private set; }

    }




    public interface ISeries
    {
        Tuple<double, double> XRange { get; }
        Tuple<double, double> YRange { get; }
        double[] XValues { get; }
        double[] YValues { get; }
        Tuple<double, double>[] Values { get; }
    }


    public interface IScheduleViewModel : IGSRoutableViewModel
    {
        Guid Id { get; }
        long? Interval { get; }
        ScheduleType Type { get; }
        DateTimeOffset ComputeNext(DateTimeOffset last);

    }

    public interface IYAxisShitViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsPageOrientation
    {
        IDictionary<MeasurementType, ISeries> Series { get; }
        IDictionary<MeasurementType, object> TelerikSeries { get; }
        IReactiveCommand ToggleSeries { get; }
        IReactiveCommand ToggleTelerikSeries { get; }

    }

    public interface IPlantActionViewModel : ICommandViewModel
    {
        string WeekDay { get; }
        string Date { get; }
        string Time { get; }
        string Note { get; }
        PlantActionType ActionType { get; }
        IconType IconType { get; }
        Guid PlantActionId { get; }
        DateTimeOffset Created { get; }

        IReactiveCommand OpenZoomView { get; }
        //PlantActionState State { get; }

        //void SetProperty(PlantActionPropertySet prop);
    }

    public interface IPlantCommentViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantMeasureViewModel : IPlantActionViewModel
    {

        MeasurementTypeViewModel Series { get; }
        double? Value { get; }
    }

    public interface IPlantWaterViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantFertilizeViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantPhotographViewModel : IPlantActionViewModel
    {

        Photo PhotoData { get; }
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

    public interface IMenuItemViewModel
    {
        IReactiveCommand Command { get; }
        object CommandParameter { get; }
        string Text { get; }
        bool IsEnabled { get; }
    }

    public interface IButtonViewModel : IMenuItemViewModel
    {
        IconType IconType { get; }
    }

    public interface IGSRoutableViewModel : IRoutableViewModel, IGSViewModel
    {
        string PageTitle { get; }
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
        SPROUTING
    }

    public enum ApplicationBarMode
    {
        DEFAULT,
        MINIMIZED
    }

}
