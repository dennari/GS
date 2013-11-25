
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System.Reactive.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{



    public class PlantViewModel : RoutableViewModel, IPlantViewModel
    {


        public IObservable<IPlantActionViewModel> PlantActionStream { get; protected set; }

        public IReactiveCommand ShareCommand { get; protected set; }
        public IReactiveCommand DeleteCommand { get; protected set; }
        public IReactiveCommand EditCommand { get; protected set; }
        public IReactiveCommand PinCommand { get; protected set; }
        public IReactiveCommand ScrollCommand { get; protected set; }


        //public IReactiveCommand FlickCommand { get; protected set; }
        //public IReactiveCommand PlantActionDetailsCommand { get; protected set; }
        //public IReactiveCommand PlantActionPivotCommand { get; protected set; }
        //public IReactiveCommand ActionTapped { get; protected set; }
        public IPlantActionViewModel SelectedItem { get; set; }
        public PlantActionType? Filter { get; set; }


        private Guid _Id;
        public Guid Id
        {
            get
            {
                return _Id;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _Id, value);
            }
        }


        private Guid _UserId;
        public Guid UserId
        {
            get
            {
                return _UserId;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _UserId, value);
            }
        }



        public PlantState State { get; protected set; }
        //public IGardenViewModel Garden { get; protected set; }






        public PlantViewModel()
            : base(null)
        {

        }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PlantViewModel(PlantState state, IGSAppViewModel app)
            : base(app)
        {




            this.WateringSchedule = new ScheduleViewModel(null, ScheduleType.WATERING, app);
            this.FertilizingSchedule = new ScheduleViewModel(null, ScheduleType.FERTILIZING, app);

            this.ShareCommand = new ReactiveCommand();
            this.DeleteCommand = new ReactiveCommand();
            this.EditCommand = Observable.Return(true).ToCommandWithSubscription(_ => this.Navigate(App.EditPlantViewModelFactory(this)));
            this.PinCommand = new ReactiveCommand();
            this.ScrollCommand = new ReactiveCommand();



            if (state != null)
            {

                this.State = state;
                this.Id = state.Id;
                this.UserId = state.UserId;

                this.ListenTo<NameSet>(this.State.Id).Select(x => x.Name)
                    .StartWith(state.Name)
                    .Subscribe(x =>
                    {
                        this.Name = x;
                    });

                this.ListenTo<SpeciesSet>(this.State.Id).Select(x => x.Species)
                    .StartWith(state.Species)
                    .Subscribe(x => this.Species = x);

                this.ListenTo<ProfilepictureSet>(this.State.Id).Select(x => x.Profilepicture)
                    .StartWith(state.Profilepicture)
                    .Subscribe(x => this.Photo = x);

                this.ListenTo<TagsSet>(this.State.Id).Select(x => (IList<string>)x.Tags.ToList())
                    .StartWith(state.Tags)
                    .Subscribe(x => this.Tags = new ReactiveList<string>(x));


                this.Photo = state.Profilepicture;


                this.App.FutureSchedules(state.Id)
                    .Subscribe(x =>
                    {
                        if (x.Type == ScheduleType.WATERING)
                            this.WateringSchedule = x;
                        else
                            this.FertilizingSchedule = x;
                    });

            }

            this.WhenAny(x => x.WateringSchedule, x => x.GetValue())
                .Where(x => x.Interval.HasValue)
                .CombineLatest(this.Actions.ItemsAdded.Where(x => x.ActionType == PlantActionType.WATERED), (schedule, axion) => Tuple.Create(schedule, axion))
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    if (x.Item1 == null || !x.Item1.Interval.HasValue)
                        return;
                    IPlantActionViewModel a = x.Item2;
                    if (a == null)
                        a = this.Actions.FirstOrDefault(y => y.ActionType == PlantActionType.WATERED);
                    if (a != null)
                    {
                        if (this.WateringScheduler == null)
                        {
                            this.WateringScheduler = new PlantScheduler(x.Item1)
                            {
                                Icon = IconType.WATER
                            };
                        }
                        this.WateringScheduler.ComputeNext(a.Created);
                    }

                });

            this.WhenAny(x => x.FertilizingSchedule, x => x.GetValue())
                .Where(x => x.Interval.HasValue)
                .CombineLatest(this.Actions.ItemsAdded.Where(x => x.ActionType == PlantActionType.FERTILIZED), (schedule, axion) => Tuple.Create(schedule, axion))
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    if (x.Item1 == null || !x.Item1.Interval.HasValue)
                        return;
                    IPlantActionViewModel a = x.Item2;
                    if (a == null)
                        a = this.Actions.FirstOrDefault(y => y.ActionType == PlantActionType.FERTILIZED);
                    if (a != null)
                    {
                        if (this.FertilizingScheduler == null)
                        {

                            this.FertilizingScheduler = new PlantScheduler(x.Item1)
                            {
                                Icon = IconType.FERTILIZE
                            };
                        }
                        this.FertilizingScheduler.ComputeNext(a.Created);
                    }
                });

            this.WhenAnyValue(x => x.FertilizingScheduler.Missed, x => x.WateringScheduler.Missed, (a, b) => a + b)
                .Subscribe(x => this.MissedCount = x);

            this.WhenAnyValue(x => x.HasTile).Subscribe(x =>
            {
                AppBarMenuItems = __AppBarMenuItems;
            });

        }


        protected bool _OwnPlant;
        public bool OwnPlant
        {
            get
            {
                return _OwnPlant;
            }
        }


        public PlantViewModel(PlantState state, IScheduleViewModel wateringSchedule, IScheduleViewModel fertilizingSchedule, IGSAppViewModel app)
            : this(state, app)
        {
            //this.WateringSchedule = wateringSchedule;
            //this.FertilizingSchedule = fertilizingSchedule;
            if (state != null)
                _OwnPlant = app.User.Id == state.UserId;

            if (wateringSchedule != null)
            {

                this.WateringSchedule = wateringSchedule;


                //this.WateringScheduler = new PlantScheduler(wateringSchedule)
                //{
                //    IconType = IconType.WATER
                //};
            }
            if (fertilizingSchedule != null)
            {
                //using (this.SuppressChangeNotifications())
                //{
                this.FertilizingSchedule = fertilizingSchedule;
                //}

                //this.FertilizingScheduler = new PlantScheduler(fertilizingSchedule);
            }

        }



        protected IScheduleViewModel _WateringSchedule;
        public IScheduleViewModel WateringSchedule
        {
            get
            {
                return _WateringSchedule;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _WateringSchedule, value);
            }
        }

        protected IScheduleViewModel _FertilizingSchedule;
        public IScheduleViewModel FertilizingSchedule
        {
            get
            {
                return _FertilizingSchedule;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _FertilizingSchedule, value);
            }
        }

        protected IReactiveList<string> _Tags;
        public IReactiveList<string> Tags
        {
            get { return _Tags; }
            set { this.RaiseAndSetIfChanged(ref _Tags, value); }
        }

        protected PlantScheduler _WateringScheduler;
        public PlantScheduler WateringScheduler
        {
            get
            {
                return _WateringScheduler;
            }
            private set { this.RaiseAndSetIfChanged(ref _WateringScheduler, value); }
        }

        protected PlantScheduler _FertilizingScheduler;
        public PlantScheduler FertilizingScheduler
        {
            get
            {
                return _FertilizingScheduler;
            }
            private set { this.RaiseAndSetIfChanged(ref _FertilizingScheduler, value); }
        }


        public string TodayWeekDay { get { return DateTimeOffset.Now.ToString("dddd"); } }
        public string TodayDate { get { return DateTimeOffset.Now.ToString("d"); } }

        protected string _Name;
        public string Name { get { return _Name; } private set { this.RaiseAndSetIfChanged(ref _Name, value); } }

        protected Photo _Photo;
        public Photo Photo
        {
            get
            {
                return _Photo;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Photo, value);
            }
        }

        private int? _MissedCount;
        public int? MissedCount
        {
            get
            {
                return _MissedCount;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _MissedCount, value);
            }
        }

        private bool _HasTile;
        public bool HasTile
        {
            get
            {
                return _HasTile;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _HasTile, value);
            }
        }



        protected string _Species;
        public string Species { get { return _Species; } private set { this.RaiseAndSetIfChanged(ref _Species, value); } }



        private IYAxisShitViewModel _Chart;
        public IYAxisShitViewModel Chart
        {
            get
            {
                return _Chart ?? (_Chart = App.YAxisShitViewModelFactory(this));
            }

        }




        private IPlantActionViewModel CreateEmptyActionVM(PlantActionType type)
        {
            var vm = App.PlantActionViewModelFactory(type);
            vm.PlantId = this.Id;
            vm.UserId = App.User.Id;
            return vm;
        }

        public IReactiveCommand _NavigateToEmptyActionCommand;
        public IReactiveCommand NavigateToEmptyActionCommand
        {

            get
            {
                if (_NavigateToEmptyActionCommand == null)
                {
                    _NavigateToEmptyActionCommand = new ReactiveCommand();
                    _NavigateToEmptyActionCommand.OfType<PlantActionType>().Subscribe(x =>
                    {
                        var vm = CreateEmptyActionVM(x);
                        vm.AddCommand.Take(1).Subscribe(_ => App.Router.NavigateBack.Execute(null));
                        this.Navigate(vm);
                    });
                }
                return _NavigateToEmptyActionCommand;
            }

        }

        public IReactiveCommand _PlantActionEdited;
        public IReactiveCommand PlantActionEdited
        {

            get
            {
                if (_PlantActionEdited == null)
                {
                    _PlantActionEdited = new ReactiveCommand();
                    _PlantActionEdited.OfType<IPlantActionViewModel>().Subscribe(x => App.Router.NavigateBack.Execute(null));
                }
                return _PlantActionEdited;
            }

        }



        protected ReactiveList<IPlantActionViewModel> _Actions;
        public IReadOnlyReactiveList<IPlantActionViewModel> Actions
        {
            get
            {
                if (_Actions == null)
                {
                    _Actions = new ReactiveList<IPlantActionViewModel>();

                    if (this.State != null)
                    {
                        var actionsPipe = App.CurrentPlantActions(this.State.Id)
                            .Concat(App.FuturePlantActions(this.State.Id));


                        actionsPipe.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
                        {
                            _Actions.Insert(0, x);

                            x.AddCommand.Subscribe(_ => this.PlantActionEdited.Execute(x));

                            var photo = x as IPlantPhotographViewModel;
                            if (photo != null)
                            {
                                photo.PhotoTimelineTap
                                    .Subscribe(_ =>
                                    {
                                        this.Filter = PlantActionType.PHOTOGRAPHED;
                                        this.SelectedItem = photo;
                                        this.Navigate(this);

                                    });
                            }

                            ScrollCommand.Execute(x);
                        });
                    }

                    //actionsPipe
                    //    .OfType<IPlantWaterViewModel>()
                    //    .Throttle(new TimeSpan(0, 0, 0, 0, 200), RxApp.TaskpoolScheduler)
                    //    .ObserveOn(RxApp.MainThreadScheduler)
                    //    .Subscribe(x => ComputeNextWatering(x));

                    //actionsPipe
                    //    .OfType<IPlantFertilizeViewModel>()
                    //    .Throttle(new TimeSpan(0, 0, 0, 0, 200), RxApp.TaskpoolScheduler)
                    //    .ObserveOn(RxApp.MainThreadScheduler)
                    //    .Subscribe(x => ComputeNextFertilizing(x));


                    //if (this.Photo == null)
                    //{
                    //    actionsPipe
                    //    .OfType<IPlantPhotographViewModel>()
                    //    .Take(1)
                    //    .Select(x => x.PhotoData)
                    //    .Subscribe(x => App.Bus.SendCommand(new SetProfilepicture(Id, x)));

                    //}

                    //App.FuturePlantActions(this.State).Subscribe(x =>
                    //{
                    //    Actions.Insert(0, x);
                    //    ScrollCommand.Execute(x);
                    //});

                }
                return _Actions;
            }
        }



        public override string UrlPathSegment
        {
            get { return string.Format("type=plant&id={0}", this.Id); }
        }

        #region APPBAR
        protected ReactiveList<IButtonViewModel> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                    _AppBarButtons = new ReactiveList<IButtonViewModel>()
                    {
                        new ButtonViewModel(null)
                        {
                            Text = "water",
                            IconType = IconType.WATER,
                            Command = Observable.Return(true).ToCommandWithSubscription( _ => {
                                var vm = CreateEmptyActionVM(PlantActionType.WATERED);
                                //vm.AddCommand.Subscribe(x => App.Router.Na)
                                
                                vm.AddCommand.Execute(null);
                            })
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "photograph",
                            IconType = IconType.PHOTO,
                            Command = NavigateToEmptyActionCommand,
                            CommandParameter = PlantActionType.PHOTOGRAPHED
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "comment",
                            IconType = IconType.NOTE,
                            Command = NavigateToEmptyActionCommand,
                            CommandParameter = PlantActionType.COMMENTED
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "share",
                            IconType = IconType.SHARE,
                            Command = ShareCommand
                        },

                    };
                return _AppBarButtons;
            }
        }



        private IReactiveCommand _ShowActionList;
        public IReactiveCommand ShowActionList
        {
            get
            {
                return _ShowActionList ?? (_ShowActionList = new ReactiveCommand());
            }

        }



        private ReactiveList<IMenuItemViewModel> __AppBarMenuItems
        {
            get
            {
                return new ReactiveList<IMenuItemViewModel>()
                    {
                        new MenuItemViewModel(null)
                        {
                            Text = "pick action",
                            Command = this.ShowActionList   
                        },                
                        new MenuItemViewModel(null)
                        {
                            Text = "edit",
                            Command = EditCommand,
                        },
                         new MenuItemViewModel(null)
                        {
                            Text = "delete",
                            Command = DeleteCommand
                        },
                        new MenuItemViewModel(null)
                        {
                            Text = HasTile ? "unpin" : "pin",
                            Command = PinCommand           
                        }
                    };
            }
        }



        protected IReadOnlyReactiveList<IMenuItemViewModel> _AppBarMenuItems;
        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get
            {
                if (_AppBarMenuItems == null)
                    _AppBarMenuItems = __AppBarMenuItems;
                return _AppBarMenuItems;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _AppBarMenuItems, value);
            }
        }

        public ApplicationBarMode AppBarMode { get { return ApplicationBarMode.DEFAULT; } }
        public bool AppBarIsVisible { get { return true; } }

        #endregion

        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }


    }

}