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

namespace Growthstories.UI.ViewModel
{


    public abstract class DesignViewModelBase : IGSRoutableViewModel, IHasAppBarButtons, IHasMenuItems, IControlsAppBar, ICommandViewModel
    {

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

        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get { return new MockReactiveList<IButtonViewModel>() { }; }
        }

        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get { return new MockReactiveList<IMenuItemViewModel>() { }; }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
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
    }

    public sealed class Series : ISeries
    {

        public Tuple<double, double> XRange { get; set; }
        public Tuple<double, double> YRange { get; set; }
        public double[] XValues { get; set; }
        public double[] YValues { get; set; }

    }

    public class PlantViewModel : DesignViewModelBase, IPlantViewModel
    {


        public IReactiveCommand PinCommand { get { return new MockReactiveCommand(); } }
        public IReactiveCommand ScrollCommand { get { return new MockReactiveCommand(); } }
        public IReactiveCommand ActionTapped { get { return new MockReactiveCommand(); } }

        public IReadOnlyReactiveList<IPlantActionViewModel> Actions { get; set; }

        public double[] Data { get; set; }


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
                new PlantPhotoViewModel(null,DateTimeOffset.Now - new TimeSpan(3,0,0,0)),
                new PlantFertilizeViewModel(DateTimeOffset.Now - new TimeSpan(4,0,0,0)),
                new PlantCommentViewModel(DateTimeOffset.Now - new TimeSpan(5,0,0,0))
            };

            this.Photo = new Photo()
            {
                LocalFullPath = @"/TestData/517e100d782a828894.jpg",
                LocalUri = @"/TestData/517e100d782a828894.jpg"
            };


            this.WateringSchedule = new ScheduleViewModel(ScheduleType.WATERING, 24 * 4 * 3600);
            this.FertilizingSchedule = new ScheduleViewModel(ScheduleType.FERTILIZING, 24 * 60 * 3600);

            this.NextWatering = new PlantWaterViewModel(this.WateringSchedule.ComputeNext(this.Actions.OfType<IPlantWaterViewModel>().First().Created));
            this.NextNourishing = new PlantFertilizeViewModel(this.FertilizingSchedule.ComputeNext(this.Actions.OfType<IPlantFertilizeViewModel>().First().Created));

            var now = DateTimeOffset.Now;
            TodayWeekDay = now.ToString("dddd");
            TodayDate = now.ToString("d");

            int num = 200;

            var s = new Series()
            {
                YValues = new double[num],
                XValues = new double[num],
                XRange = Tuple.Create((double)0, 6 * Math.PI),
                YRange = Tuple.Create((double)0, (double)1)
            };

            double step = (s.XRange.Item2 - s.XRange.Item1) / num;
            double x = s.XRange.Item1;
            //s.XValues[0] = x;
            for (var i = 0; i < num; i++)
            {
                s.YValues[i] = Math.Sin(x);
                s.XValues[i] = x;
                x += step;
            }

            this.Series = new[] { s };

        }

        public PlantViewModel(string name, string species)
            : this()
        {

            Name = name;
            Species = species;


        }


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



        public IEnumerable<ISeries> Series { get; set; }
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
                        new PlantViewModel("Jare","Aloe Vera"),
                        new PlantViewModel("Sepi","Aloe Vera"),
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



        public IAuthUser UserState
        {
            get { throw new NotImplementedException(); }
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

}
