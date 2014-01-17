
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Growthstories.UI.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;


namespace Growthstories.UI.ViewModel
{



    public class PlantViewModel : RoutableViewModel, IPlantViewModel
    {

        #region COMMANDS
        public IObservable<IPlantActionViewModel> PlantActionStream { get; protected set; }

        public IReactiveCommand ShareCommand { get; protected set; }
        public IReactiveCommand TryShareCommand { get; protected set; }
        public IReactiveCommand WateringCommand { get; protected set; }
        public IObservable<IPlantViewModel> WateringObservable { get; protected set; }

        public IReactiveCommand PhotoCommand { get; protected set; }
        public IReactiveCommand DeleteCommand { get; protected set; }
        public IObservable<IPlantViewModel> DeleteObservable { get; protected set; }


        public IReactiveCommand EditCommand { get; protected set; }
        public IReactiveCommand PinCommand { get; protected set; }
        public IReactiveCommand ScrollCommand { get; protected set; }
        public IReactiveCommand ResetAnimationsCommand { get; protected set; }

        #endregion



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



        private bool _IsShared;
        public bool IsShared
        {
            get
            {
                return _IsShared;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsShared, value);
            }
        }


        private bool _IsWateringScheduleEnabled;
        public bool IsWateringScheduleEnabled
        {
            get
            {
                return _IsWateringScheduleEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsWateringScheduleEnabled, value);
                this.raisePropertyChanged("ShowWateringScheduler");
                this.ShowTileNotification = IsWateringScheduleEnabled
                    && WateringScheduler != null && WateringScheduler.MissedLateAndOwn;
            }
        }


        private bool _IsFertilizingScheduleEnabled;
        public bool IsFertilizingScheduleEnabled
        {
            get
            {
                return _IsFertilizingScheduleEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsFertilizingScheduleEnabled, value);
                this.raisePropertyChanged("ShowFertilizingScheduler");
            }
        }


        public bool OwnPlant { get; protected set; }


        private bool _HasWriteAccess;
        public bool HasWriteAccess
        {
            get
            {
                return _HasWriteAccess;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _HasWriteAccess, value);
            }
        }



        public PlantViewModel(IObservable<Tuple<PlantState, ScheduleState, ScheduleState>> stateObservable, IGSAppViewModel app)
            : base(app)
        {


            if (stateObservable == null)
                throw new ArgumentNullException("StateObservable cannot be null");

            stateObservable.Take(1).Subscribe(x => this.Init(x));

            this.WateringSchedule = new ScheduleViewModel(null, ScheduleType.WATERING, app);
            this.WateringScheduler = new PlantScheduler(WateringSchedule, OwnPlant) { Icon = IconType.WATER };
            var emptyWatering = CreateEmptyActionVM(PlantActionType.WATERED);
            this.WateringCommand = emptyWatering.AddCommand;
            this.WateringObservable = emptyWatering.AsyncAddObservable.Select(_ => this);



            this.FertilizingSchedule = new ScheduleViewModel(null, ScheduleType.FERTILIZING, app);

            ResetAnimationsCommand = new ReactiveCommand();

            this.TryShareCommand = new ReactiveCommand();
            TryShareCommand.Where(_ => App.EnsureDataConnection()).Subscribe(_ =>
            {


                if (App.User.IsRegistered)
                {
                    this.ShareCommand.Execute(null);

                }
                else
                {
                    var svm = new SignInRegisterViewModel(App)
                    {
                        SignInMode = false
                    };
                    svm.Response
                        .Where(x => SignInRegisterViewModel.IsSuccess(x))
                        .Take(1)
                        .Subscribe(x =>
                        {
                            this.NavigateBack();
                            this.ShareCommand.Execute(null);
                        });

                    this.Navigate(svm);
                }
            });

            var canShare = Observable.CombineLatest(
                     this.WhenAnyValue(x => x.IsShared),
                     App.WhenAnyValue(x => x.IsRegistered),
                     (a, b) => a && b);
            this.ShareCommand = new ReactiveCommand(canShare);

            Observable.CombineLatest(
                this.WhenAnyValue(x => x.UserId),
                App.WhenAnyValue(x => x.User),
                (a, b) => Tuple.Create(a, b)
              ).Subscribe(x =>
              {
                  this.HasWriteAccess = x.Item2 != null && x.Item1 == x.Item2.Id;
              });




            this.DeleteCommand = new ReactiveCommand();
            var obs = this.DeleteCommand.RegisterAsyncTask(_ => SendCommand(new DeleteAggregate(this.Id, "plant"))).Publish();
            this.DeleteObservable = obs.Select(_ => this);
            obs.Connect();

            this.PhotoCommand = Observable.Return(true).ToCommandWithSubscription(_ =>
            {
                var vm = (IPlantPhotographViewModel)CreateEmptyActionVM(PlantActionType.PHOTOGRAPHED);
                vm.PhotoChooserCommand.Execute(null);
                vm.WhenAnyValue(x => x.Photo).Subscribe(x =>
                {
                    if (x != null)
                        vm.AddCommand.Execute(null);
                });
                //vm.AddCommand.Execute(null);
            });


            this.EditCommand = Observable.Return(true).ToCommandWithSubscription(_ => this.Navigate(App.EditPlantViewModelFactory(this)));
            this.PinCommand = new ReactiveCommand();
            this.ScrollCommand = new ReactiveCommand();



            var actionsAccessed = this.WhenAnyValue(x => x.ActionsAccessed).Where(x => x).Take(1);

            actionsAccessed.SelectMany(_ =>
            {

                return Observable.CombineLatest(
                    this.WhenAnyValue(y => y.WateringSchedule).Where(y => y != null),
                    this.Actions.ItemsAdded.StartWith(this.Actions).Where(x => x.ActionType == PlantActionType.WATERED),
                    (x, y) => Tuple.Create(x, y)
                 );
            }).Subscribe(x =>
            {
                if (this.WateringScheduler == null || this.WateringScheduler.Schedule != x.Item1)
                {
                    this.WateringScheduler = new PlantScheduler(x.Item1, OwnPlant) { Icon = IconType.WATER };

                    this.WateringScheduler.WhenAnyValue(y => y.MissedLateAndOwn)
                    .Subscribe(z =>
                    {
                        ShowTileNotification = z && IsWateringScheduleEnabled;
                    });

                }
                this.WateringScheduler.LastActionTime = x.Item2.Created;
            });

            actionsAccessed.SelectMany(_ =>
            {
                return Observable.CombineLatest(
                    this.WhenAnyValue(y => y.FertilizingSchedule).Where(y => y != null),
                    this.Actions.ItemsAdded.StartWith(this.Actions).Where(x => x.ActionType == PlantActionType.FERTILIZED),
                    (x, y) => Tuple.Create(x, y)
                 );
            }).Subscribe(x =>
            {
                if (this.FertilizingScheduler == null || this.FertilizingScheduler.Schedule != x.Item1)
                    this.FertilizingScheduler = new PlantScheduler(x.Item1, OwnPlant) { Icon = IconType.FERTILIZE };
                this.FertilizingScheduler.LastActionTime = x.Item2.Created;
            });

            this.WhenAnyValue(x => x.FertilizingScheduler.Missed, x => x.WateringScheduler.Missed, (a, b) => a + b)
            .Subscribe(x =>
            {
                this.MissedCount = (int?)x;
            });

            this.WhenAnyValue(x => x.HasTile).Subscribe(x =>
            {
                AppBarMenuItems = __AppBarMenuItems;
            });

            this.WhenAnyValue(x => x.IsWateringScheduleEnabled, x => x.WateringScheduler, (x, y) => x && y != null)
                .ToProperty(this, x => x.ShowWateringScheduler, out _ShowWateringScheduler);

            this.WhenAnyValue(x => x.IsFertilizingScheduleEnabled, x => x.FertilizingScheduler, (x, y) => x && y != null)
                .ToProperty(this, x => x.ShowFertilizingScheduler, out _ShowFertilizingScheduler);




        }



        private void Init(Tuple<PlantState, ScheduleState, ScheduleState> stateTuple)
        {

            var state = stateTuple.Item1;

            this.State = state;
            this.Id = state.Id;
            this.UserId = state.UserId;
            this.IsShared = state.Public;
            this.WateringSchedule = stateTuple.Item2 != null ? new ScheduleViewModel(stateTuple.Item2, ScheduleType.WATERING, App) : null;
            this.FertilizingSchedule = stateTuple.Item3 != null ? new ScheduleViewModel(stateTuple.Item3, ScheduleType.FERTILIZING, App) : null;
            this.IsFertilizingScheduleEnabled = state.IsFertilizingScheduleEnabled;
            this.IsWateringScheduleEnabled = state.IsWateringScheduleEnabled;


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

            if (state.Profilepicture == null && state.ProfilepictureActionId.HasValue)
            {
                var photoId = state.ProfilepictureActionId.Value;
                this.Actions.ItemsAdded.OfType<IPlantPhotographViewModel>().Where(x => x.PlantActionId == photoId).Take(1).Subscribe(x =>
                {
                    this.Photo = x.Photo;
                });
            }


            this.ListenTo<MarkedPlantPublic>(this.State.Id)
                .Subscribe(x => this.IsShared = true);

            this.ListenTo<MarkedPlantPrivate>(this.State.Id)
                .Subscribe(x => this.IsShared = false);

            this.ListenTo<ScheduleToggled>(this.State.Id)
                .Subscribe(x =>
                {
                    if (x.Type == ScheduleType.WATERING)
                        this.IsWateringScheduleEnabled = x.IsEnabled;
                    else
                        this.IsFertilizingScheduleEnabled = x.IsEnabled;

                });

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


            // we need these right away for tile notifications (specially with the WP start screen tiles)
            //if (state != null)
            //{
            //    var latest = App.UIPersistence.GetLatestWatering(state.Id);
            //    if (latest != null)
            //    {
            //        this.WateringScheduler.LastActionTime = latest.Created;
            //    }
            //}
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

        private bool _ActionsAccessed;
        public bool ActionsAccessed
        {
            get
            {
                return _ActionsAccessed;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ActionsAccessed, value);
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
            private set
            {
                this.RaiseAndSetIfChanged(ref _WateringScheduler, value);
            }
        }

        protected PlantScheduler _FertilizingScheduler;
        public PlantScheduler FertilizingScheduler
        {
            get
            {
                return _FertilizingScheduler;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _FertilizingScheduler, value);
            }
        }


        private ObservableAsPropertyHelper<bool> _ShowFertilizingScheduler;
        public bool ShowFertilizingScheduler
        {
            get
            {
                return _ShowFertilizingScheduler.Value;
            }
        }

        private ObservableAsPropertyHelper<bool> _ShowWateringScheduler;
        public bool ShowWateringScheduler
        {
            get
            {
                return _ShowWateringScheduler.Value;
            }
        }


        public string TodayWeekDay { get { return SharedViewHelpers.FormatWeekDay(DateTimeOffset.Now); } }
        public string TodayDate { get { return DateTimeOffset.Now.ToString("d"); } }

        protected string _Name;
        public string Name { get { return _Name; } protected set { this.RaiseAndSetIfChanged(ref _Name, value); } }

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
                // what's this? -Ville
                return (int)(new DateTime().Ticks % 10000);
                //return _MissedCount;
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
        public string Species { get { return _Species; } protected set { this.RaiseAndSetIfChanged(ref _Species, value); } }



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


        public int PlantIndex { get; set; }


        protected ReactiveList<IPlantActionViewModel> _Actions;
        public IReadOnlyReactiveList<IPlantActionViewModel> Actions
        {
            get
            {
                if (_Actions == null)
                {
                    _Actions = new ReactiveList<IPlantActionViewModel>();
                    this.ActionsAccessed = true;
                    if (this.State != null)
                    {
                        var actionsPipe = App.CurrentPlantActions(this.State.Id)
                            .Concat(App.FuturePlantActions(this.State.Id));


                        actionsPipe.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
                        {
                            //_Actions.Add(x);
                            x.PlantId = this.Id;
                            x.UserId = this.UserId;
                            x.ActionIndex = 0;
                            _Actions.Insert(0, x);

                            foreach (var a in _Actions)
                            {
                                a.ActionIndex++;
                            }

                            x.AddCommand.Subscribe(_ => this.PlantActionEdited.Execute(x));
                            //x.DeleteCommand.Subscribe(_ => _Actions.Remove(x));

                            this.ListenTo<AggregateDeleted>(x.PlantActionId)
                              .Subscribe(y =>
                              {
                                  _Actions.Remove(x);
                              });

                            var photo = x as IPlantPhotographViewModel;
                            if (photo != null)
                            {
                                photo.PhotoTimelineTap
                                    .Subscribe(_ =>
                                    {
                                        this.Navigate(new PhotoListViewModel(this.Actions.OfType<IPlantPhotographViewModel>().ToList(), App, photo));
                                    });
                            }

                            //ScrollCommand.Execute(x);
                        });
                    }


                }
                return _Actions;
            }
        }


        private bool _ShowTileNotification;
        public bool ShowTileNotification
        {
            get
            {
                return _ShowTileNotification;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _ShowTileNotification, value);
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
                    _AppBarButtons = App.User.Id == this.UserId ? GetOwnerButtons() : GetFollowerButtons();
                return _AppBarButtons;
            }
        }



        private ReactiveList<IButtonViewModel> GetFollowerButtons()
        {
            return new ReactiveList<IButtonViewModel>();
        }

        private ReactiveList<IButtonViewModel> GetOwnerButtons()
        {
            return new ReactiveList<IButtonViewModel>()
                    {
                        new ButtonViewModel(null)
                        {
                            Text = "water",
                            IconType = IconType.WATER,
                            Command = this.WateringCommand,
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "photograph",
                            IconType = IconType.PHOTO,
                            Command = this.PhotoCommand
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
                            Command = TryShareCommand
                        },

                    };
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
                ReactiveList<IMenuItemViewModel> ret = new ReactiveList<IMenuItemViewModel>()
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
                                        
                    };


                ret.Add
                (
                    new MenuItemViewModel(null)
                    {
                        Text = "delete",
                        Command = Observable.Return(true).ToCommandWithSubscription(_ =>
                        {
                            var popup = DeleteConfirmation();
                            popup.DismissedObservable
                                .Where(x => x == PopupResult.LeftButton)
                                .Take(1)
                                .Subscribe(__ =>
                                {
                                    this.DeleteCommand.Execute(null);
                                    this.NavigateBack();

                                });
                            App.ShowPopup.Execute(popup);
                        })
                    }
                );


                ret.Add
                (
                  new MenuItemViewModel(null)
                        {
                            Text = HasTile ? "unpin" : "pin",
                            Command = PinCommand
                        }
                );

                return ret;
            }
        }



        protected IReadOnlyReactiveList<IMenuItemViewModel> _AppBarMenuItems;
        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get
            {
                if (_AppBarMenuItems == null)
                {
                    if (this.UserId == App.User.Id)
                        _AppBarMenuItems = __AppBarMenuItems;
                }
                return _AppBarMenuItems;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _AppBarMenuItems, value);
            }
        }

        public ApplicationBarMode AppBarMode { get { return ApplicationBarMode.DEFAULT; } }
        public virtual bool AppBarIsVisible
        {
            get
            {
                /*
                if (SelectedItem != null && Filter == PlantActionType.PHOTOGRAPHED)
                {
                    return false;
                }
                */

                return UserId == App.User.Id;
            }
        }

        #endregion

        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }



        public IPopupViewModel DeleteConfirmation()
        {
            return new PopupViewModel()
            {
                Caption = "Confirm delete",
                Message = "Are you sure you wish to delete the plant "
                        + Name.ToUpper()
                        + "? This can't be undone.",

                IsLeftButtonEnabled = true,
                IsRightButtonEnabled = true,
                LeftButtonContent = "Yes",
                RightButtonContent = "Cancel"
            };
        }



    }


    public class EmptyPlantViewModel : IPlantViewModel
    {

        private static EmptyPlantViewModel _Instance;

        public static EmptyPlantViewModel Instance
        {
            get
            {
                return _Instance ?? (_Instance = new EmptyPlantViewModel());
            }
        }

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Name { get; set; }

        public string Species { get; set; }

        public IReactiveCommand PinCommand { get; set; }

        public IReactiveCommand ShareCommand { get; set; }

        public IReactiveCommand ScrollCommand { get; set; }

        public IReactiveCommand WateringCommand { get; set; }

        public IObservable<IPlantViewModel> WateringObservable { get; set; }

        public IReactiveCommand DeleteCommand { get; set; }

        public IReactiveCommand NavigateToEmptyActionCommand { get; set; }

        public IReactiveCommand ShowActionList { get; set; }

        public IReactiveCommand ResetAnimationsCommand { get; set; }

        public IReactiveList<string> Tags { get; set; }

        public Photo Photo { get; set; }

        public int? MissedCount { get; set; }

        public bool HasTile { get; set; }

        public bool IsShared { get; set; }

        public bool IsFertilizingScheduleEnabled { get; set; }

        public bool IsWateringScheduleEnabled { get; set; }

        public IReadOnlyReactiveList<IPlantActionViewModel> Actions { get; set; }

        public IPlantActionViewModel SelectedItem { get; set; }

        public IScheduleViewModel WateringSchedule { get; set; }

        public IScheduleViewModel FertilizingSchedule { get; set; }

        public IYAxisShitViewModel Chart { get; set; }

        public PlantScheduler WateringScheduler { get; set; }

        public PlantScheduler FertilizingScheduler { get; set; }

        public string TodayWeekDay { get; set; }

        public string TodayDate { get; set; }

        public int PlantIndex { get; set; }

        public string UrlPath { get; set; }

        public string UrlPathSegment { get; set; }

        public IScreen HostScreen { get; set; }

        public IObservable<IObservedChange<object, object>> Changing { get; set; }

        public IObservable<IObservedChange<object, object>> Changed { get; set; }

        public IDisposable SuppressChangeNotifications()
        {
            return Disposable.Empty;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons { get; set; }

        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems { get; set; }

        public ApplicationBarMode AppBarMode { get; set; }

        public bool AppBarIsVisible { get; set; }

        public SupportedPageOrientation SupportedOrientations { get; set; }
    }


}