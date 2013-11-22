using Growthstories.Domain.Entities;
using Growthstories.Sync;
using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Windows.Media.Imaging;
using Growthstories.Domain.Messaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Growthstories.UI.ViewModel
{
    public static class Helpers
    {

        public static Random RandomGen = new Random();


    }

    public abstract class DesignViewModelBase : IGSRoutableViewModel, IHasAppBarButtons, IHasMenuItems, IControlsAppBar, ICommandViewModel, INotifyPropertyChanged
    {

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {


            var handler = this.PropertyChanged;
            if (handler != null && propertyName != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }


        }

        public string PageTitle
        {
            get { return "PageTitle"; }
        }

        public IGSAppViewModel App
        {
            get { return null; }
        }

        public IObservable<IObservedChange<object, object>> Changing
        {
            get { return null; }
        }

        public IObservable<IObservedChange<object, object>> Changed
        {
            get { return null; }
        }

        public IDisposable SuppressChangeNotifications()
        {
            return null;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

        public virtual IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get { return new MockReactiveList<IButtonViewModel>() { }; }
        }

        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get { return new MockReactiveList<IMenuItemViewModel>() { }; }
        }

        public virtual ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public virtual bool AppBarIsVisible
        {
            get { return true; }
        }

        public IReactiveCommand AddCommand
        {
            get { return new MockReactiveCommand(); }
        }

        public IObservable<bool> CanExecute
        {
            get { return Observable.Return(true); }
        }

        public string TopTitle
        {
            get { return "TopTitle"; }
        }

        public string Title
        {
            get { return "Title"; }
        }

        public string UrlPathSegment
        {
            get { return "sdfs"; }
        }

        public IScreen HostScreen
        {
            get { return null; }
        }

        //event PropertyChangingEventHandler INotifyPropertyChanging.PropertyChanging
        //{
        //    add { throw new NotImplementedException(); }
        //    remove { throw new NotImplementedException(); }
        //}
    }


    public class PlantViewModel : DesignViewModelBase, IPlantViewModel
    {


        public IReactiveCommand PinCommand { get { return new MockReactiveCommand(); } }
        public IReactiveCommand ScrollCommand { get { return new MockReactiveCommand(); } }
        public IReactiveCommand ActionTapped { get { return new MockReactiveCommand(); } }

        public IReadOnlyReactiveList<IPlantActionViewModel> Actions { get; set; }



        public PlantViewModel()
            : base()
        {
            Name = "Sepi";
            Species = "Aloe Vera";
            this.Actions = new MockReactiveList<IPlantActionViewModel>()
            {
                new PlantMeasureViewModel(DateTimeOffset.Now - new TimeSpan(1,0,0,0)),
                new PlantPhotoViewModel(null, DateTimeOffset.Now),                
                new PlantWaterViewModel(DateTimeOffset.Now - new TimeSpan(2,0,0,0)),                
                new PlantPhotoViewModel(@"/TestData/flowers-from-the-conservatory.jpg",DateTimeOffset.Now - new TimeSpan(3,0,0,0)),
                new PlantFertilizeViewModel(DateTimeOffset.Now - new TimeSpan(4,0,0,0)),
                new PlantCommentViewModel(DateTimeOffset.Now - new TimeSpan(5,0,0,0))
            };

            //this.Photo = new Photo()
            //{
            //    LocalFullPath = @"/TestData/517e100d782a828894.jpg",
            //    LocalUri = @"/TestData/517e100d782a828894.jpg"
            //};

            var photos = this.Actions.OfType<PlantPhotoViewModel>().Select(x => x.Photo).ToArray();
            this.Photo = photos[Helpers.RandomGen.Next(0, photos.Length)];

            this.WateringSchedule = new ScheduleViewModel(ScheduleType.WATERING, 24 * 4 * 3600);
            this.FertilizingSchedule = new ScheduleViewModel(ScheduleType.FERTILIZING, 24 * 60 * 3600);



            this.WateringScheduler = new PlantScheduler(WateringSchedule)
            {
                Icon = IconType.WATER
            };

            var missedSpan = new TimeSpan(Helpers.RandomGen.Next(2) > 0 ? 12 : 0, 0, 0, 0);
            this.WateringScheduler.ComputeNext(DateTimeOffset.UtcNow - missedSpan);
            this.FertilizingScheduler = new PlantScheduler(FertilizingSchedule)
            {
                Icon = IconType.FERTILIZE
            };
            this.FertilizingScheduler.ComputeNext(DateTimeOffset.UtcNow - new TimeSpan(12, 0, 0, 0));

            var now = DateTimeOffset.Now;
            TodayWeekDay = now.ToString("dddd");
            TodayDate = now.ToString("d");



        }

        public PlantViewModel(string name, string species)
            : this()
        {

            Name = name;
            Species = species;


        }

        public int? NumMissed { get; private set; }

        public Guid Id
        {
            get { return Guid.NewGuid(); }
        }

        public Guid UserId
        {
            get { return Guid.NewGuid(); }
        }

        public string PlantTitle
        {
            get { return Name; }
        }

        public string Name { get; set; }


        public string Header
        {
            get { return Name; }
        }

        public string Species { get; set; }


        public IScheduleViewModel WateringSchedule { get; set; }
        public IScheduleViewModel FertilizingSchedule { get; set; }
        public IPlantWaterViewModel NextWatering { get; set; }
        public IPlantFertilizeViewModel NextNourishing { get; set; }

        public Photo Photo { get; set; }

        public string TodayWeekDay { get; set; }
        public string TodayDate { get; set; }



        public string UrlPathSegment
        {
            get { return "sdfsdf"; }
        }



        public SupportedPageOrientation SupportedOrientations
        {
            get { throw new NotImplementedException(); }
        }





        public IPlantActionViewModel SelectedItem { get; set; }





        public PlantScheduler WateringScheduler { get; set; }

        public PlantScheduler FertilizingScheduler { get; set; }



        public IReactiveList<string> Tags
        {
            get { return new MockReactiveList<string>(); }
        }


        public IReactiveCommand AddActionCommand(PlantActionType type)
        {
            return new MockReactiveCommand();
        }
    }


    public class ScheduleViewModel : DesignViewModelBase, IScheduleViewModel
    {
        public ScheduleType Type { get; private set; }



        public ScheduleViewModel(ScheduleType scheduleType, long? interval = null)
        {
            this.Type = scheduleType;


            if (interval != null && interval.HasValue)
            {
                this.Interval = TimeSpan.FromSeconds(interval.Value);
                this.IsEnabled = true;
            }



        }

        public ScheduleViewModel()
            : this(ScheduleType.WATERING, 24 * 50 * 3600)
        {



        }

        protected TimeSpan? _Interval;
        public TimeSpan? Interval
        {
            get
            {
                return _Interval;
            }
            set
            {
                if (value != _Interval && value.HasValue)
                {
                    _Interval = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged("IntervalLabel");
                }

            }
        }

        public IconType OKIcon
        {
            get
            {
                return IconType.CHECK;
            }
        }

        public IconType CancelIcon
        {
            get
            {
                return IconType.CANCEL;
            }
        }

        public string IntervalLabel
        {
            get
            {
                if (Interval.HasValue)
                {
                    var t = Interval.Value;
                    //int w = t.TotalWeeks(), h = t.

                    if (t.TotalWeeks() > 0)
                        return string.Format("{0:D} weeks, {1:D} days", t.TotalWeeks(), t.DaysAfterWeeks());
                    if (t.Days > 0)
                        return string.Format("{0:%d} days, {0:%h} hours", t);
                    return string.Format("{0:%h} hours", t);

                }
                return "Not set";
            }
        }



        public string ScheduleTypeLabel
        {
            get
            {
                return string.Format("{0}", this.Type == ScheduleType.WATERING ? "watering schedule" : "fertilizing schedule");
            }
        }


        public bool IsEnabled { get; set; }




        public DateTimeOffset ComputeNext(DateTimeOffset last)
        {
            if (Interval == null)
                throw new InvalidOperationException("This schedule is unspecified, so the next occurence cannot be computed.");
            return last + Interval.Value;
        }



        public Guid? Id { get; set; }


        protected string _Label;
        public string Label
        {
            get
            {
                return _Label;
            }
        }

        protected Tuple<IPlantViewModel, IScheduleViewModel> _CopySchedule;
        public Tuple<IPlantViewModel, IScheduleViewModel> CopySchedule
        {
            get
            {
                return _CopySchedule;
            }
            set
            {
                if (value != null && value != _CopySchedule)
                {
                    _CopySchedule = value;
                    this.RaisePropertyChanged();
                    this.Interval = value.Item2.Interval;
                }
            }
        }

        //protected Tuple<IPlantViewModel, IScheduleViewModel> _CopySchedule;
        public object SelectedCopySchedule
        {
            get
            {
                return _CopySchedule;
            }
            set
            {
                var v = value as Tuple<IPlantViewModel, IScheduleViewModel>;
                if (v != null)
                    CopySchedule = v;
            }
        }


        protected IReactiveList<Tuple<IPlantViewModel, IScheduleViewModel>> _OtherSchedules;
        public IReactiveList<Tuple<IPlantViewModel, IScheduleViewModel>> OtherSchedules
        {
            get
            {
                return _OtherSchedules;
            }
            set
            {
                if (value != null && value != _OtherSchedules)
                {
                    _OtherSchedules = value;
                    this.RaisePropertyChanged();
                    if (value.Count() > 0)
                        HasOtherSchedules = true;
                    else
                        HasOtherSchedules = false;
                }
            }
        }

        protected bool _HasOtherSchedules;
        public bool HasOtherSchedules
        {
            get
            {
                return _HasOtherSchedules;
            }
            set
            {
                if (value != _HasOtherSchedules)
                {
                    _HasOtherSchedules = value;
                    this.RaisePropertyChanged();
                }
            }
        }




        public Task<Schedule> Create()
        {
            return null;

        }



    }



    public sealed class IntervalValue
    {
        private readonly IntervalValueType Type;
        public IntervalValue(IntervalValueType type)
        {
            this.Type = type;
        }

        public long Compute(string sValue)
        {
            var dValue = double.Parse(sValue);
            return (long)(dValue * this.Unit);
        }

        public Guid Id
        {
            get { return Guid.NewGuid(); }
        }

        public string Compute(long lValue)
        {
            return (lValue / Unit).ToString("F1");
        }

        public long Unit
        {
            get
            {
                return this.Type == IntervalValueType.DAY ? 24 * 3600 : 3600;
            }
        }


        public string Title
        {
            get { return this.Type == IntervalValueType.DAY ? "days" : "hours"; }
        }

        public override string ToString()
        {
            return this.Title;
        }

        public static IList<IntervalValue> GetAll()
        {
            return new List<IntervalValue>()
            {
                new IntervalValue(IntervalValueType.DAY),
                new IntervalValue(IntervalValueType.HOUR)
            };
        }


    }

    public enum IntervalValueType
    {
        HOUR,
        DAY
    }



    public sealed class GardenViewModel : DesignViewModelBase, IGardenViewModel
    {

        public bool IsPlantSelectionEnabled { get { return false; } }
        public IPlantViewModel SelectedItem { get; set; }


        public Guid Id { get; set; }

        public IReadOnlyReactiveList<IPlantViewModel> Plants { get; set; }

        public string Username { get; set; }


        public GardenViewModel()
        {
            Load();
        }

        private void Load()
        {
            Username = "Jaakko";
            Plants = new MockReactiveList<IPlantViewModel>()
                    {
                        new PlantViewModel("Pelargonia","Aloe Vera"),
                        new PlantViewModel("Orkkideemus","Aloe Vera"),
                        new PlantViewModel("Tero","Aloe Vera")

                    };
        }

        public GardenViewModel(bool isEmpty)
        {
            if (!isEmpty)
            {
                Load();
            }
        }



        public IAuthUser User
        {
            get { throw new NotImplementedException(); }
        }



        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }


        public IReactiveCommand SelectedItemsChanged
        {
            get { return new MockReactiveCommand(); }
        }
    }



    public sealed class SearchUsersViewModel : DesignViewModelBase, ISearchUsersViewModel
    {

        public IReactiveCommand SearchCommand { get { return new MockReactiveCommand(); } }
        public IReactiveCommand UserSelectedCommand { get { return new MockReactiveCommand(); } }

        public IReadOnlyReactiveList<RemoteUser> List { get; private set; }

        public SearchUsersViewModel()
        {


        }


        public bool ProgressIndicatorIsVisible
        {
            get { return false; }
        }

        public bool SystemTrayIsVisible
        {
            get { return true; }
        }
    }


    public sealed class PlantActionItem
    {
        public IconType Icon { get; set; }
        public string Title { get; set; }
        public IReactiveCommand Command { get; set; }

    }

    public sealed class PlantActionListViewModel : DesignViewModelBase, IPlantActionListViewModel
    {



        public IReadOnlyReactiveList<PlantActionItem> _PlantActions;
        public IReadOnlyReactiveList<PlantActionItem> PlantActions
        {
            get
            {
                return _PlantActions;
            }
            private set { _PlantActions = value; }
        }

        public List<string> Test
        {
            get
            {
                return new List<string>() { "testline", "another testlin" };
            }
        }

        public PlantActionListViewModel()
        {

            this.PlantActions = new MockReactiveList<PlantActionItem>() 
            {
                new PlantActionItem() {
                    Icon = IconType.WATER,
                    Title = "water",
                    Command = new MockReactiveCommand()
                },
                new PlantActionItem() {
                    Icon = IconType.FERTILIZE,
                    Title = "nourish",
                    Command = new MockReactiveCommand()
                },
                new PlantActionItem() {
                    Icon = IconType.PHOTO,
                    Title = "photo",
                    Command = new MockReactiveCommand()
                },
                new PlantActionItem() {
                    Icon = IconType.MEASURE,
                    Title = "measure",
                    Command = new MockReactiveCommand()
                },
                new PlantActionItem() {
                    Icon = IconType.MISTING,
                    Title = "mist",
                    Command = new MockReactiveCommand()
                }             
            };
        }


    }



    public class FriendsViewModel : DesignViewModelBase, IFriendsViewModel
    {


        public IReactiveCommand FriendTapped { get { return new MockReactiveCommand(); } }

        public IReadOnlyReactiveList<IGardenViewModel> Friends { get; set; }


        public FriendsViewModel()
        {

            this.Friends = new MockReactiveList<IGardenViewModel>()
            {
                new GardenViewModel(true) 
                {
                    Username = "Jaakko",
                    Plants = new MockReactiveList<IPlantViewModel>()
                    {
                        new PlantViewModel("Jare","Aloe Vera"),
                        new PlantViewModel("Sepi","Aloe Vera"),
                        new PlantViewModel("Tero","Aloe Vera")

                    }

                },
                new GardenViewModel(true) 
                {
                    Username = "Lauri",
                    Plants = new MockReactiveList<IPlantViewModel>()
                    {
                        new PlantViewModel("Jare","Aloe Vera"),
                        new PlantViewModel("Sepi","Aloe Vera"),
                        new PlantViewModel("Tero","Aloe Vera")

                    }

                }

            };
        }


        public IGardenViewModel SelectedFriend
        {
            get { return Friends[0]; }
        }

        public object SelectedItem
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }



    public class ButtonViewModel : MenuItemViewModel, IButtonViewModel
    {
        public ButtonViewModel()
        {

        }


        /// <summary>
        /// Gets or sets the icon URI.
        /// </summary>
        /// <value>
        /// The icon URI.
        /// </value>
        public IconType IconType { get; set; }

    }




    public class MenuItemViewModel : IMenuItemViewModel
    {

        public MenuItemViewModel()
        {

        }

        #region Command
        private IReactiveCommand command;

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public IReactiveCommand Command
        {
            get { return this.command; }
            set { this.command = value; }
        }
        #endregion

        #region CommandParameter
        private object commandParameter;

        /// <summary>
        /// Gets or sets the command's parameter.
        /// </summary>
        /// <value>
        /// The command's parameter.
        /// </value>
        public object CommandParameter
        {
            get { return this.commandParameter; }
            set { this.commandParameter = value; }
        }
        #endregion

        #region Text
        private string text;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }
        #endregion

        #region IsEnabled
        private bool _IsEnabled = true;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public bool IsEnabled
        {
            get { return this._IsEnabled; }
            set { this.IsEnabled = value; }
        }
        #endregion
    }
}
