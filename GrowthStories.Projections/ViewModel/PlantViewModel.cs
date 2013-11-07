
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
        public IReactiveCommand PinCommand { get; protected set; }
        public IReactiveCommand ScrollCommand { get; protected set; }
        public IReactiveCommand FlickCommand { get; protected set; }
        public IReactiveCommand PlantActionDetailsCommand { get; protected set; }
        public IReactiveCommand PlantActionPivotCommand { get; protected set; }
        public IReactiveCommand ActionTapped { get; protected set; }
        public IPlantActionViewModel SelectedItem { get; set; }
        public PlantActionType? Filter { get; set; }

        protected ReactiveList<IPlantActionViewModel> _FilteredActions;
        public IReadOnlyReactiveList<IPlantActionViewModel> FilteredActions
        {
            get
            {
                if (_FilteredActions == null)
                    _FilteredActions = !Filter.HasValue ? (ReactiveList<IPlantActionViewModel>)Actions : new ReactiveList<IPlantActionViewModel>(Actions.Where(x => x.ActionType == Filter.Value));
                return _FilteredActions;
            }
        }

        public Guid Id { get { return State.Id; } }
        public Guid UserId { get { return State.UserId; } }


        public PlantState State { get; protected set; }
        public IGardenViewModel Garden { get; protected set; }



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
            //this.ActionProjection = actionProjection;
            //this.ActionProjection.EventHandled += this.ActionHandled;
            //this.Actions = new ObservableCollection<ActionBase>();
            if (state == null)
                throw new ArgumentNullException("PlantState has to be given in PlantViewModel");
            this.State = state;



            this.ShareCommand = new ReactiveCommand();
            this.DeleteCommand = new ReactiveCommand();
            this.PinCommand = new ReactiveCommand();
            this.ScrollCommand = new ReactiveCommand();
            this.FlickCommand = new ReactiveCommand();
            this.PlantActionDetailsCommand = new ReactiveCommand();
            this.PlantActionDetailsCommand
                .OfType<IPlantActionViewModel>()
                .Subscribe(x =>
                {
                    x.AddCommand.Subscribe(_ =>
                    {
                        var cmd = new SetPlantActionProperty(x.PlantActionId)
                        {
                            Note = x.Note,
                        };
                        var m = x as IPlantMeasureViewModel;
                        if (m != null)
                        {
                            cmd.Value = m.Value;
                        }

                        this.SendCommand(cmd, true);
                    });
                    this.Navigate(x);
                });
            this.PlantActionPivotCommand = new ReactiveCommand();
            this.PlantActionPivotCommand
                .OfType<IPlantActionViewModel>()
                .Subscribe(x =>
                {
                    x.AddCommand.Subscribe(_ =>
                    {
                        var cmd = new SetPlantActionProperty(x.PlantActionId)
                        {
                            Note = x.Note,
                        };
                        var m = x as IPlantMeasureViewModel;
                        if (m != null)
                        {
                            cmd.Value = m.Value;
                        }

                        this.SendCommand(cmd, true);
                    });
                    this.Navigate(x);
                });
            this.ActionTapped = new ReactiveCommand();
            this.ActionTapped
                .OfType<IPlantPhotographViewModel>()
                .Subscribe(x =>
                {
                    this.Filter = PlantActionType.PHOTOGRAPHED;
                    this.SelectedItem = x;
                    this.Navigate(this);

                });


            this.ScrollCommand = new ReactiveCommand();

            this.ListenTo<NameSet>(this.State.Id).Select(x => x.Name)
                .ToProperty(this, x => x.Name, out this._Name, state.Name);

            this.ListenTo<ProfilepictureSet>(this.State.Id).Select(x => x.Profilepicture)
                .ToProperty(this, x => x.Photo, out this._ProfilepictureData, state.Profilepicture);

            this.ListenTo<SpeciesSet>(this.State.Id).Select(x => x.Species)
               .ToProperty(this, x => x.Species, out this._Species, state.Species);



            //this.App.WhenAny(x => x.Orientation, x => x.GetValue())
            //    .CombineLatest(this.App.Router.CurrentViewModel.Where(x => x == this), (x, cvm) => ((x & PageOrientation.Landscape) == PageOrientation.Landscape))
            //    .Where(x => x == true)
            //    .Subscribe(_ => App.Router.Navigate.Execute(App.YAxisShitViewModelFactory(this)));



        }

        public PlantViewModel(PlantState state, IScheduleViewModel wateringSchedule, IScheduleViewModel fertilizingSchedule, IGSAppViewModel app)
            : this(state, app)
        {
            //this.WateringSchedule = wateringSchedule;
            //this.FertilizingSchedule = fertilizingSchedule;

            this.App.FutureSchedules(state.Id)
                .Where(x =>
                {
                    return x.Type == ScheduleType.WATERING;
                })
                .ToProperty(this, x => x.WateringSchedule, out this._WateringSchedule, wateringSchedule);

            this.App.FutureSchedules(state.Id).Where(x => x.Type == ScheduleType.FERTILIZING)
                .ToProperty(this, x => x.FertilizingSchedule, out this._FertilizingSchedule, fertilizingSchedule);


            this.WhenAny(x => x.WateringSchedule, x => x.GetValue()).Subscribe(x =>
            {
                var a = this.Actions.FirstOrDefault(y => y.ActionType == PlantActionType.WATERED);
                if (a != null)
                    this.NextWatering = new PlantWaterViewModel(x.ComputeNext(a.Created), App);
            });

            this.WhenAny(x => x.FertilizingSchedule, x => x.GetValue()).Subscribe(x =>
            {
                var a = this.Actions.FirstOrDefault(y => y.ActionType == PlantActionType.FERTILIZED);
                if (a != null)
                    this.NextNourishing = new PlantFertilizeViewModel(x.ComputeNext(a.Created), App);
            });



        }

        protected ObservableAsPropertyHelper<IScheduleViewModel> _WateringSchedule;
        public IScheduleViewModel WateringSchedule
        {
            get
            {
                return _WateringSchedule.Value;
            }
        }

        protected ObservableAsPropertyHelper<IScheduleViewModel> _FertilizingSchedule;
        public IScheduleViewModel FertilizingSchedule
        {
            get
            {
                return _FertilizingSchedule.Value;
            }
        }


        IPlantWaterViewModel _NextWatering;
        public IPlantWaterViewModel NextWatering
        {
            get
            {
                return _NextWatering;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _NextWatering, value);
            }
        }

        IPlantFertilizeViewModel _NextNourishing;
        public IPlantFertilizeViewModel NextNourishing
        {
            get
            {
                return _NextNourishing;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _NextNourishing, value);
            }
        }

        public string TodayWeekDay { get; set; }
        public string TodayDate { get; set; }

        protected ObservableAsPropertyHelper<string> _Name;
        public string Name
        {
            get
            {
                return _Name.Value;
            }
        }

        protected ObservableAsPropertyHelper<Photo> _ProfilepictureData;
        public Photo Photo
        {
            get
            {
                return _ProfilepictureData.Value;
            }
        }

        protected ObservableAsPropertyHelper<string> _Species;
        public string Species
        {
            get
            {
                return _Species.Value;
            }
        }


        //private ObservableAsPropertyHelper<IPlantWaterViewModel> _LatestWatering;
        //public _LatestWatering

        private ObservableAsPropertyHelper<IPlantFertilizeViewModel> LatestFertilizing;



        protected ReactiveList<IPlantActionViewModel> _Actions;
        public IReadOnlyReactiveList<IPlantActionViewModel> Actions
        {
            get
            {
                if (_Actions == null)
                {
                    _Actions = new ReactiveList<IPlantActionViewModel>();
                    var actionsPipe = App.CurrentPlantActions(this.State)
                        .Concat(App.FuturePlantActions(this.State));


                    actionsPipe.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
                    {
                        _Actions.Insert(0, x);
                        ScrollCommand.Execute(x);
                    });

                    actionsPipe
                        .OfType<IPlantWaterViewModel>()
                        .Throttle(new TimeSpan(0, 0, 0, 0, 200), RxApp.TaskpoolScheduler)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => ComputeNextWatering(x));

                    actionsPipe
                        .OfType<IPlantFertilizeViewModel>()
                        .Throttle(new TimeSpan(0, 0, 0, 0, 200), RxApp.TaskpoolScheduler)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => ComputeNextFertilizing(x));


                    //App.FuturePlantActions(this.State).Subscribe(x =>
                    //{
                    //    Actions.Insert(0, x);
                    //    ScrollCommand.Execute(x);
                    //});

                }
                return _Actions;
            }
        }

        private void ComputeNextFertilizing(IPlantFertilizeViewModel a)
        {
            var x = this.WateringSchedule;
            if (x != null)
                this.NextWatering = new PlantWaterViewModel(x.ComputeNext(a.Created), App);
        }

        private void ComputeNextWatering(IPlantWaterViewModel a)
        {
            var x = this.FertilizingSchedule;
            if (x != null)
                this.NextNourishing = new PlantFertilizeViewModel(x.ComputeNext(a.Created), App);
        }

        protected IRoutableViewModel _AddPlantViewModel;
        public IRoutableViewModel AddPlantViewModel
        {
            get
            {
                return _AddPlantViewModel ?? (_AddPlantViewModel = App.AddPlantViewModelFactory(this.State));
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
                            IconUri = App.IconUri[IconType.WATER],
                            Command = Observable.Return(true)
                                .ToCommandWithSubscription(x => {
                                    var vm = new PlantWaterViewModel(null, App);
                                    vm.AddCommand.Subscribe(_ =>
                                    {
                                        //this.SendCommand(new Water(this.UserId, this.Id, _AddWaterViewModel.Note), true);
                                        this.SendCommand(
                                            new CreatePlantAction(
                                                Guid.NewGuid(),
                                                this.UserId,
                                                this.Id,
                                                PlantActionType.WATERED,
                                                vm.Note
                                            ),
                                        true);
                                    });
                                    this.Navigate(vm);
                                })
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "photograph",
                            IconUri = App.IconUri[IconType.PHOTO],
                            Command = Observable.Return(true)
                                .ToCommandWithSubscription(x =>  {
                                    var vm = new PlantPhotographViewModel(null, App);
                                    vm.AddCommand.Subscribe(_ =>
                                    {
                                        //this.SendCommand(new Water(this.UserId, this.Id, _AddWaterViewModel.Note), true);
                                        this.SendCommand(
                                            new CreatePlantAction(
                                                Guid.NewGuid(),
                                                this.UserId,
                                                this.Id,
                                                PlantActionType.PHOTOGRAPHED,
                                                vm.Note
                                            ) {
                                                Photo = vm.PhotoData
                                            },
                                        true);
                                    });
                                    this.Navigate(vm);
                                })
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "comment",
                            IconUri = App.IconUri[IconType.NOTE],
                            Command = Observable.Return(true)
                                .ToCommandWithSubscription(x =>  {
                                    var vm = new PlantCommentViewModel(null, App);
                                    vm.AddCommand.Subscribe(_ =>
                                    {
                                        //this.SendCommand(new Water(this.UserId, this.Id, _AddWaterViewModel.Note), true);
                                        this.SendCommand(
                                            new CreatePlantAction(
                                                Guid.NewGuid(),
                                                this.UserId,
                                                this.Id,
                                                PlantActionType.COMMENTED,
                                                vm.Note
                                            ),
                                        true);
                                    });
                                    this.Navigate(vm);
                                })
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "share",
                            IconUri = App.IconUri[IconType.SHARE],
                            Command = ShareCommand
                        },

                    };
                return _AppBarButtons;
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
                            Text = "measure",
                            Command = Observable.Return(true)
                               .ToCommandWithSubscription(x =>  {
                                    var vm = new PlantMeasureViewModel(null, App);
                                    vm.AddCommand.Subscribe(_ =>
                                    {
                                        //this.SendCommand(new Water(this.UserId, this.Id, _AddWaterViewModel.Note), true);
                                        this.SendCommand(
                                            new CreatePlantAction(
                                                Guid.NewGuid(),
                                                this.UserId,
                                                this.Id,
                                                PlantActionType.MEASURED,
                                                vm.Note
                                     ) {
                                        MeasurementType = vm.Series.Type,
                                        Value = vm.Value.Value
                                    },
                                        true);
                                    });
                                    this.Navigate(vm);
                                })
                        },
                        new MenuItemViewModel(null)
                        {
                            Text = "nourish",
                            Command = Observable.Return(true)
                                .ToCommandWithSubscription(x =>  {
                                    var vm = new PlantFertilizeViewModel(null, App);
                                    vm.AddCommand.Subscribe(_ =>
                                    {
                                        //this.SendCommand(new Water(this.UserId, this.Id, _AddWaterViewModel.Note), true);
                                        this.SendCommand(
                                            new CreatePlantAction(
                                                Guid.NewGuid(),
                                                this.UserId,
                                                this.Id,
                                                PlantActionType.FERTILIZED,
                                                vm.Note
                                            ),
                                        true);
                                    });
                                    this.Navigate(vm);
                                })
                        },
                        new MenuItemViewModel(null)
                        {
                            Text = "edit",
                            Command = Observable.Return(true)
                                .ToCommandWithSubscription(_ => this.Navigate(this.AddPlantViewModel)),
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