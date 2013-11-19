using Growthstories.Domain.Entities;
using Growthstories.Sync;
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

            var photos = this.Actions.OfType<PlantPhotoViewModel>().Select(x => x.PhotoData).ToArray();
            this.Photo = photos[Helpers.RandomGen.Next(0, photos.Length)];

            this.WateringSchedule = new ScheduleViewModel(ScheduleType.WATERING, 24 * 4 * 3600);
            this.FertilizingSchedule = new ScheduleViewModel(ScheduleType.FERTILIZING, 24 * 60 * 3600);



            this.WateringScheduler = new PlantScheduler(WateringSchedule)
            {
                IconType = IconType.WATER
            };

            var missedSpan = new TimeSpan(Helpers.RandomGen.Next(2) > 0 ? 12 : 0, 0, 0, 0);
            this.WateringScheduler.ComputeNext(DateTimeOffset.UtcNow - missedSpan);
            this.FertilizingScheduler = new PlantScheduler(FertilizingSchedule)
            {
                IconType = IconType.FERTILIZE
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

    }




    public class ScheduleViewModel : DesignViewModelBase, IScheduleViewModel
    {



        public ScheduleViewModel(ScheduleType scheduleType, long interval)
        {
            this.Interval = interval;
            this.Type = scheduleType;
        }



        public Guid Id
        {
            get { return Guid.NewGuid(); }
        }

        public long? Interval { get; set; }


        public DateTimeOffset ComputeNext(DateTimeOffset last)
        {
            if (Interval == null)
                throw new InvalidOperationException("This schedule is unspecified, so the next occurence cannot be computed.");
            return last + new TimeSpan((long)(Interval * 10000 * 1000));
        }


        public ScheduleType Type { get; set; }

        public IReactiveCommand SelectValueType { get { return new MockReactiveCommand(); } }
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

        public IGardenViewModel SelectedItem
        {
            get { return Friends[0]; }
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
