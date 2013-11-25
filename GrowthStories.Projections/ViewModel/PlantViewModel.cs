
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


        public Guid Id { get { return State.Id; } }
        public Guid UserId { get { return State.UserId; } }


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
            this.EditCommand = Observable.Return(true)
                                .ToCommandWithSubscription(_ => this.Navigate(App.EditPlantViewModelFactory(this)));
            this.PinCommand = new ReactiveCommand();
            this.ScrollCommand = new ReactiveCommand();
            //this.FlickCommand = new ReactiveCommand();
            //this.PlantActionDetailsCommand = new ReactiveCommand();
            //this.PlantActionDetailsCommand
            //    .OfType<IPlantActionViewModel>()
            //    .Subscribe(x =>
            //    {
            //        x.AddCommand.Subscribe(_ =>
            //        {
            //            var cmd = new SetPlantActionProperty(x.PlantActionId)
            //            {
            //                Note = x.Note,
            //            };
            //            var m = x as IPlantMeasureViewModel;
            //            if (m != null)
            //            {
            //                cmd.Value = m.Value;
            //            }

            //            this.SendCommand(cmd, true);
            //        });
            //        this.Navigate(x);
            //    });
            //this.PlantActionPivotCommand = new ReactiveCommand();
            //this.PlantActionPivotCommand
            //    .OfType<IPlantActionViewModel>()
            //    .Subscribe(x =>
            //    {
            //        x.AddCommand.Subscribe(_ =>
            //        {
            //            var cmd = new SetPlantActionProperty(x.PlantActionId)
            //            {
            //                Note = x.Note,
            //            };
            //            var m = x as IPlantMeasureViewModel;
            //            if (m != null)
            //            {
            //                cmd.Value = m.Value;
            //            }

            //            this.SendCommand(cmd, true);
            //        });
            //        this.Navigate(x);
            //    });
            //this.ActionTapped = new ReactiveCommand();
            //this.ActionTapped
            //    .OfType<IPlantPhotographViewModel>()
            //    .Subscribe(x =>
            //    {
            //        this.Filter = PlantActionType.PHOTOGRAPHED;
            //        this.SelectedItem = x;
            //        this.Navigate(this);

            //    });


            this.ScrollCommand = new ReactiveCommand();


            if (state != null)
            {

                this.State = state;
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

        protected string _Species;
        public string Species { get { return _Species; } private set { this.RaiseAndSetIfChanged(ref _Species, value); } }


        //private ObservableAsPropertyHelper<IPlantWaterViewModel> _LatestWatering;
        //public _LatestWatering

        //private ObservableAsPropertyHelper<IPlantActionViewModel> LatestFertilizing;

        private void PlantActionNavigateBackSubscription(IReactiveCommand cmd)
        {
            cmd.Take(1).Subscribe(_ =>
            {
                var lastI = App.Router.NavigationStack.Count - 1;
                IRoutableViewModel vm = null;
                var steps = 1;
                try
                {
                    if (App.Router.NavigationStack[lastI - 1] is IPlantActionListViewModel)
                        steps = 2;
                }
                catch
                {

                }
                for (var i = 0; i < steps; i++)
                    App.Router.NavigateBack.Execute(null);
            });
        }


        private IPlantActionViewModel CreateEmptyActionVM(PlantActionType type)
        {
            var vm = App.PlantActionViewModelFactory(type);
            vm.PlantId = this.Id;
            vm.UserId = App.User.Id;
            return vm;
        }

        public IReactiveCommand AddActionCommand(PlantActionType type)
        {
            var vm = CreateEmptyActionVM(type);
            PlantActionNavigateBackSubscription(vm.AddCommand);

            return Observable.Return(true).ToCommandWithSubscription(_ => this.Navigate(vm));
        }


        public IReactiveCommand EditActionCommand(IPlantActionViewModel vm)
        {

            PlantActionNavigateBackSubscription(vm.AddCommand);
            return Observable.Return(true).ToCommandWithSubscription(_ => this.Navigate(vm));

        }


        //protected IReactiveDerivedList<ITimelineActionViewModel> _TimelineActions;
        //public IReadOnlyReactiveList<ITimelineActionViewModel> TimelineActions
        //{
        //    get
        //    {
        //        if (_TimelineActions == null)
        //        {
        //            _TimelineActions = this.Actions.CreateDerivedCollection(x =>
        //            {
        //                var vm = new TimelineActionViewModel(x);
        //                vm.EditCommand = this.EditActionCommand(x);
        //                return (ITimelineActionViewModel)vm;
        //            });
        //        }
        //        return _TimelineActions;
        //    }
        //}

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
                            x.EditCommand = this.EditActionCommand(x);

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
            get { throw new NotImplementedException(); }
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
                                vm.AddCommand.Execute(null);
                            })
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "photograph",
                            IconType = IconType.PHOTO,
                            Command = AddActionCommand(PlantActionType.PHOTOGRAPHED)
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "comment",
                            IconType = IconType.NOTE,
                            Command = AddActionCommand(PlantActionType.COMMENTED)
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

        private IPlantActionListViewModel _PlantActionList;
        protected IPlantActionListViewModel PlantActionList
        {
            get
            {
                if (_PlantActionList == null)
                {
                    _PlantActionList = new PlantActionListViewModel(this, App);
                }
                return _PlantActionList;
            }
        }

        protected ReactiveList<IMenuItemViewModel> _AppBarMenuItems;
        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get
            {
                if (_AppBarMenuItems == null)
                    _AppBarMenuItems = new ReactiveList<IMenuItemViewModel>()
                    {
                        new MenuItemViewModel(null)
                        {
                            Text = "pick action",
                            Command = Observable.Return(true).ToCommandWithSubscription(_ => this.Navigate(PlantActionList))
   
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
                            Text = "pin",
                            Command = PinCommand,
                            CommandParameter = this.State
                        }
                    };
                return _AppBarMenuItems;
            }
        }

        public ApplicationBarMode AppBarMode { get { return ApplicationBarMode.DEFAULT; } }
        public bool AppBarIsVisible { get { return true; } }






        #endregion









        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }




        public IEnumerable<ISeries> Series
        {
            get { throw new NotImplementedException(); }
        }

    }

}