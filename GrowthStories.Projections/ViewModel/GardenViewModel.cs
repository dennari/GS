
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using ReactiveUI;
using Growthstories.UI.Services;


namespace Growthstories.UI.ViewModel
{


    public class GardenViewModel : RoutableViewModel, IGardenViewModel
    {



        public override string ToString()
        {
            return string.Format("Garden of user {0} (instance of class {1})", this.Username, "GardenViewModel");
        }

        private bool _PlantsLoaded;
        public bool PlantsLoaded
        {
            get
            {
                return _PlantsLoaded;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _PlantsLoaded, value);
            }
        }

        private HashSet<IPlantViewModel> MultiDeleteList = new HashSet<IPlantViewModel>();
        private HashSet<IPlantViewModel> SelfDeleteList = new HashSet<IPlantViewModel>();


        private ObservableAsPropertyHelper<IPlantViewModel[]> _PlantsItemsSource;
        public IPlantViewModel[] PlantsItemsSource
        {
            get
            {
                return _PlantsItemsSource != null ? _PlantsItemsSource.Value : null;
            }
        }


        private void IntroducePlant(IPlantViewModel x)
        {
            x.PlantIndex = _Plants.Count();
            x.ShowDetailsCommand = this.ShowDetailsCommand;

            _Plants.Add(x);

            x.DeleteRequestedCommand.OfType<DeleteRequestOrigin>().Where(y => y == DeleteRequestOrigin.SELF).Subscribe(y =>
            {
                this.SelfDeleteList.Add(x);
            });

        }


        private void LoadPlants(IAuthUser u)
        {

            PlantsLoaded = true;
            var current = App.CurrentPlants(u.Id)
                .ObserveOn(RxApp.MainThreadScheduler);


            //current
            //    .SelectMany(x => x.WhenAnyValue(y => y.Loaded))
            //    .Where(x => x)

            current.Subscribe(x => IntroducePlant(x), () =>
            {
                SubscribeForNestedIsLoaded();
            });

            //if (!OwnGarden) // future plants to own garden are added directly by the return value of AddEditPlantViewModel
            //{
            subs.Add(App.FuturePlants(u.Id)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => IntroducePlant(x)));
            //}
        }


        public bool ShouldPlantBeFullyLoaded(IPlantViewModel plant, IPlantViewModel selectedPlant)
        {
            if (plant.UserId != selectedPlant.UserId)
            {
                return false;
            }

            //this.Log().Info(
            //    "index for plant is {0}, for selectedplant is {1}, count is {2}", 
            //    plant.PlantIndex, selectedPlant.PlantIndex, Plants.Count());

            return
                selectedPlant == plant
                || selectedPlant.PlantIndex + 1 == plant.PlantIndex
                || selectedPlant.PlantIndex - 1 == plant.PlantIndex
                || (selectedPlant.PlantIndex == 0 && plant.PlantIndex == Plants.Count() - 1)
                || (selectedPlant.PlantIndex == Plants.Count() - 1 && plant.PlantIndex == 0);
        }


        private void SubscribeForNestedIsLoaded()
        {
            var count = Plants.Count();
            if (count == 0)
            {
                IsLoaded = true;
                App.AppPrettyMuchLoaded = true;
                return;
            }

            Plants
                .ToObservable()
                .SelectMany(x => x.WhenAnyValue(y => y.Loaded).Where(y => y).Take(1))
                .Subscribe(_ => { }, () =>
                {
                    this.Log().Info("Garden and its plants are loaded");
                    IsLoaded = true;
                    App.AppPrettyMuchLoaded = true;
                });
        }


        protected ReactiveList<IPlantViewModel> _Plants;
        public IReadOnlyReactiveList<IPlantViewModel> Plants
        {
            get
            {
                if (_Plants == null)
                {
                    _Plants = new ReactiveList<IPlantViewModel>()
                    {
                        ChangeTrackingEnabled = true
                    };
                    if (this.User != null)
                    {
                        LoadPlants(this.User);
                    }
                    else
                    {
                        this.WhenAny(x => x.User, x => x.GetValue()).Where(x => x != null).Take(1).Subscribe(x => LoadPlants(x));
                    }
                }
                return _Plants;
            }
            private set
            {
                // we reset this if all the plants in the garden get deleted
                this.RaiseAndSetIfChanged(ref _Plants, (ReactiveList<IPlantViewModel>)value);
            }
        }


        private bool _IsLoaded;
        public bool IsLoaded
        {
            get
            {
                return _IsLoaded;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsLoaded, value);
            }
        }


        protected string _Username;
        public string Username
        {
            get { return _Username; }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _Username, value);
            }
        }

        protected ReactiveList<IPlantViewModel> _SelectedPlants = new ReactiveList<IPlantViewModel>();
        public IReadOnlyReactiveList<IPlantViewModel> SelectedPlants { get { return _SelectedPlants; } }
        //public IPlantViewModel SelectedItem { get; set; }



        public IReactiveCommand SelectedItemsChanged { get; protected set; }
        public IReactiveCommand ShowDetailsCommand { get; protected set; }
        public IReactiveCommand GetPlantCommand { get; protected set; }

        public IReactiveCommand TryAddPlantCommand { get; private set; }

        public IReactiveCommand MultiWateringCommand { get; private set; }
        public IReactiveCommand MultiDeleteCommand { get; private set; }

        public IObservable<bool> IsNotInProgress { get; private set; }

        //public PlantProjection PlantProjection { get; private set; }
        // public GardenState State { get; protected set; }
        protected IAuthUser _User;
        public IAuthUser User
        {
            get
            {

                return _User;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _User, value);
            }
        }

        public string PlantTitle { get; protected set; }

        public Guid Id { get; protected set; }
        public Guid UserId { get; protected set; }

        private readonly IIAPService IAP;
        private readonly Func<ISettingsViewModel> SettingsF;
        private readonly Func<IAddEditPlantViewModel> AddPlantViewModelF;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public GardenViewModel(
            IObservable<IAuthUser> stateObservable,
            bool isOwn,
            IGSAppViewModel app,
            IIAPService iap = null,
            Func<ISettingsViewModel> settings = null,
            Func<IAddEditPlantViewModel> addPlant = null
            )
            : base(app)
        {
            IsLoaded = false;

            this.SettingsF = settings;
            this.AddPlantViewModelF = addPlant;
            this.IAP = iap;

            SelectedItemsChanged = new ReactiveCommand();
            SelectedItemsChanged.Subscribe(p =>
            {
                var ar = p as Tuple<IList, IList>;
                if (ar != null)
                {

                    if (ar.Item1 != null && ar.Item1.Count > 0)
                    {
                        foreach (var plant in ar.Item1.Cast<IPlantViewModel>())
                            _SelectedPlants.Add(plant);
                    }
                    if (ar.Item2 != null && ar.Item2.Count > 0)
                    {
                        foreach (var plant in ar.Item2.Cast<IPlantViewModel>())
                            _SelectedPlants.Remove(plant);
                    }
                }
            });

            TryAddPlantCommand = new ReactiveCommand();
            TryAddPlantCommand.RegisterAsyncTask(_ => TryAddPlant());

            this.ShowDetailsCommand = new ReactiveCommand();
            this.ShowDetailsCommand
                .OfType<IPlantViewModel>()
                .Subscribe(x =>
                {
                    this.Log().Info("showdetailscommand: navigating to " + x.Name);
                    this.PivotVM.SelectedItem = x;
                    App.Router.Navigate.Execute(this.PivotVM);
                });

            this.IsNotInProgress = this.WhenAnyValue(x => x.MultiCommandInFlight, x => !x);


            this.MultiWateringCommand = new ReactiveCommand(this.IsNotInProgress);
            this.MultiWateringCommand
                .Select(_ => new { list = _SelectedPlants, count = _SelectedPlants.Count })

                .Where(x => x.count > 0)
                .Do(_ => MultiCommandInFlight = true)
                //.SelectMany(x => x.list.ToObservable().Select((y, i) => new { el = y, i = i, of = x.count }))

                .SelectMany(x =>
                {
                    this.Log().Info("MultiWatering started");
                    return x.list.ToObservable().SelectMany(y => App.HandleCommand(new CreatePlantAction(Guid.NewGuid(), UserId, y.Id, PlantActionType.WATERED, null)))
                    .Aggregate(0, (c, y) => c++);


                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    this.Log().Info("MultiWatering ended");
                    MultiCommandInFlight = false;
                    this.IsPlantSelectionEnabled = false;
                    this._SelectedPlants.Clear();


                });


            this.MultiDeleteCommand = new ReactiveCommand(this.IsNotInProgress);
            this.MultiDeleteCommand
                .Select(_ => new { list = _SelectedPlants, count = _SelectedPlants.Count })
                .Where(x => x.count > 0)
                .Select(x =>
                {
                    var popup = MultiDeleteConfirmation(x.count);
                    App.ShowPopup.Execute(popup);
                    return popup.AcceptedObservable.Take(1).Select(_ => x);
                })
                .Switch()
                .Do(_ =>
                {
                    MultiCommandInFlight = true;
                    this.MultiDeleteList = new HashSet<IPlantViewModel>(_.list);
                })
                //.SelectMany(x => x.list.ToObservable().Select((y, i) => new { el = y, i = i, of = x.count }))
                .SelectMany(x =>
                {
                    this.Log().Info("MultiDelete started");

                    return x.list.ToObservable()
                        .Do(y => y.DeleteCommand.Execute(null))
                        .Aggregate(0, (c, y) => c++);

                    //await App.HandleCommand(new DeleteAggregate(x.el.Id, "plant"));

                    //x.el.DeleteCommand.Execute(null);
                    //return this.ListenTo<AggregateDeleted>(x.el.Id).Take(1).Select(_ => x);
                    //foreach(var plant in x.)
                    //return x;
                    //return Observable.Merge(x.list.Select(y =>
                    //{
                    //    y.DeleteCommand.Execute(null);
                    //    return this.ListenTo<AggregateDeleted>(y.Id).Take(1);
                    //})).Aggregate(0, (c, y) => c++);
                    //return Observable.Concat()
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    this.IsPlantSelectionEnabled = false;
                    MultiCommandInFlight = false;
                    this._SelectedPlants.Clear();
                    this.Log().Info("MultiDelete ended");

                });

            this.WhenAnyValue(x => x.IsPlantSelectionEnabled).Subscribe(x =>
            {
                if (x)
                    _SelectedPlants.Clear();
            });

            var notInProgressAndSelected = this.IsNotInProgress.CombineLatest(this._SelectedPlants.IsEmptyChanged, (x, y) => x && !y);
            this.DeletePlantsButtonCommand = new ReactiveCommand(notInProgressAndSelected, false);
            this.DeletePlantsButtonCommand.Subscribe(_ => this.MultiDeleteCommand.Execute(null));
            this.WaterPlantsButtonCommand = new ReactiveCommand(notInProgressAndSelected, false);
            this.WaterPlantsButtonCommand.Subscribe(_ => this.MultiWateringCommand.Execute(null));

            subs.Add(App.BackKeyPressedCommand.OfType<CancelEventArgs>().Subscribe(x =>
            {
                if (this.IsPlantSelectionEnabled)
                {
                    x.Cancel = true;
                    this.IsPlantSelectionEnabled = false;
                }
            }));

            stateObservable
                .Where(x => x != null)
                .Take(1)
                .Subscribe(x =>
            {
                Init(x, isOwn);
            });

            subs.Add(App.WhenAnyValue(x => x.SelectedPlant).Where(x => x != null).Subscribe(x =>
            {
                foreach (var p in Plants)
                {
                    var should = ShouldPlantBeFullyLoaded(p, x);
                    //this.Log().Info("should plant {0} be fully loaded is now {1}", p.Name, should);
                    p.ShouldBeFullyLoaded = should;
                }
            }));

            var curr = App.Router.GetCurrentViewModel();

            CanBeUnloadedObservable = App.Router.CurrentViewModel.Where(x => !(x is IFriendsViewModel));

            subs.Add(this.WhenAnyValue(x => x.Plants).Where(x => x != null).Subscribe(__ =>
            {
                PlantsItemsSourceSubs.Dispose();
                PlantsItemsSourceSubs = this.Plants.Changed
                   .Throttle(TimeSpan.FromMilliseconds(250), RxApp.TaskpoolScheduler)
                   .ObserveOn(RxApp.MainThreadScheduler)
                   .Select(_ => this.Plants.ToArray())
                   .ToProperty(this, y => y.PlantsItemsSource, out _PlantsItemsSource);
            }));
        }

        private IDisposable PlantsItemsSourceSubs = Disposable.Empty;

        // Signal that the view corresponding to this viewmodel can be unloaded
        //
        public IObservable<IRoutableViewModel> CanBeUnloadedObservable { get; set; }


        public IPopupViewModel MultiDeleteConfirmation(int count)
        {
            string msg;
            if (count == 1)
            {
                msg = String.Format(
                    "Are you sure you wish to delete the plant {0}? This can't be undone.",
                    SelectedPlants.First().Name.ToUpper());
            }
            else
            {
                msg = string.Format("Are you sure you wish to delete {0} plants? This can't be undone.", count);
            }

            return new PopupViewModel()
            {
                Caption = "Confirm delete",
                Message = msg,
                IsLeftButtonEnabled = true,
                IsRightButtonEnabled = false,
                LeftButtonContent = "Yes",
            };
        }


        private bool _MultiCommandInFlight;
        public bool MultiCommandInFlight
        {
            get
            {
                return _MultiCommandInFlight;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _MultiCommandInFlight, value);
            }
        }



        protected async Task TryAddPlant()
        {
            // note: if listing information is incorrect user can add more plants for free
            if (Plants.Count == 3 && !IAP.HasPaidBasicProduct() && await IAP.FormattedPrice() != null)
            {
                var pvm = new PopupViewModel()
                {
                    Caption = "Purchase",
                    Message = "You are currently limited to 3 plants. Add 4 additional plants for only "
                            + await IAP.FormattedPrice() + "!",
                    IsLeftButtonEnabled = true,
                    IsRightButtonEnabled = true,
                    LeftButtonContent = "Buy",
                    RightButtonContent = "Not now"
                };

                pvm.AcceptedObservable
                    .Take(1)
                    .SelectMany(async _ =>
                    {
                        var r = await IAP.ShopForBasicProduct();
                        return r;
                    })
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(AfterIAP);

                App.ShowPopup.Execute(pvm);

            }
            else if (Plants.Count >= 7)
            {

                var pvm = new PopupViewModel()
                {
                    Caption = "Can't add more plants",
                    Message = "You can have a total of 7 plants.",
                    IsLeftButtonEnabled = true,
                    LeftButtonContent = "OK",
                };

                App.ShowPopup.Execute(pvm);

            }
            else
            {
                AfterIAP(true);
            }
        }

        private IDisposable CreatedPlantsSubscription = Disposable.Empty;
        protected void AfterIAP(bool bought)
        {
            if (bought)
            {
                //this.WhenAnyValue(x => x.AddPlantViewModel).Take(1).Subscribe(this.Navigate);
                var addPlantVM = App.EditPlantViewModelFactory(null);
                CreatedPlantsSubscription.Dispose();
                CreatedPlantsSubscription = addPlantVM.CreatedPlants.Take(1).ObserveOn(RxApp.MainThreadScheduler).Subscribe(IntroducePlant);
                this.Navigate(addPlantVM);

            }
            // else, user did not buy anything so
            // we are not going to navigate anywhere
        }


        protected IGardenPivotViewModel _PivotVM;
        public IGardenPivotViewModel PivotVM
        {
            get
            {
                if (_PivotVM == null)
                {
                    _PivotVM = new GardenPivotViewModel(this, App);
                }
                return _PivotVM;

            }
        }


        private bool _OwnGarden = true;
        public bool OwnGarden
        {
            get
            {
                return _OwnGarden;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _OwnGarden, value);
            }
        }


        private void Init(IAuthUser user, bool isOwn)
        {
            this.User = user;
            this.Username = user.Username;
            this.Id = user.GardenId;
            this.UserId = user.Id;
            this.OwnGarden = isOwn;
            this.AppBarIsVisible = isOwn;

            //this.State = state.Garden;

            this.PlantTitle = string.Format("{0}'s garden", User.Username).ToUpper();

            if (isOwn)
            {
                var TileModeDefaultButtons = new ReactiveList<IButtonViewModel>()
                {
                    AddPlantButton,
                    SettingsButton
                };
                var TileModeDefaultButtonsWithSelection = new ReactiveList<IButtonViewModel>()
                {
                    AddPlantButton,
                    SelectPlantsButton,
                    SettingsButton
                };
                var currentTileModeDefaultButtons = TileModeDefaultButtons;

                var TileModeMultiCommandButtons = new ReactiveList<IButtonViewModel>()
            {
                    WaterPlantsButton,
                    DeletePlantsButton
                };

                //var afterPlantsLoaded = this.WhenAnyValue(x => x.PlantsLoaded).Where(x => x).Take(1);
                //var afterPlantsLoadedCount = afterPlantsLoaded.SelectMany(_ => Plants.CountChanged.StartWith(Plants.Count));
                //var afterPlantsLoadedEmptyChanged = afterPlantsLoaded.SelectMany(_ => _Plants.IsEmptyChanged).Select(x => _Plants.Count);

                //var moreThanZeroChanged = Observable.Zip(afterPlantsLoadedCount, afterPlantsLoadedCount.Skip(1), (x, y) => new { current = x, prev = y })
                //   .Where(x => x.current + x.prev == 1).Select(x => x.current - x.prev)


                IDisposable shouldAddMultiselectButtonSubscription = this.WhenAnyValue(x => x.PlantsLoaded)
                    .Where(x => x)
                    .Take(1)
                    .SelectMany(_ => this.WhenAnyValue(x => x.Plants).Select(x => (ReactiveList<IPlantViewModel>)x))
                    .Select(x => x.IsEmptyChanged.StartWith(x.IsEmpty))
                    .Switch()
                    .Subscribe(x =>
                    {
                        currentTileModeDefaultButtons = !x ? TileModeDefaultButtonsWithSelection : TileModeDefaultButtons;
                        if (!IsPlantSelectionEnabled)
                        {
                            this.AppBarButtons = currentTileModeDefaultButtons;
                        }
                    });

                //.Do(x => prevCount = x).Where(x => (x > 1 && prevCount <= 1) || ()
                this.WhenAnyValue(x => x.IsPlantSelectionEnabled)
                  .Subscribe(x =>
                  {
                      if (x)
                      {
                          this.Log().Info("PlantSelectionMode enabled");
                          this.AppBarButtons = TileModeMultiCommandButtons;
                          AppBarMode = ApplicationBarMode.DEFAULT;

                      }
                      else
                      {
                          this.Log().Info("PlantSelectionMode disabled");
                          AppBarMode = ApplicationBarMode.MINIMIZED;
                          this.AppBarButtons = currentTileModeDefaultButtons;
                      }
                  });


                subs.Add(this.ListenTo<AggregateDeleted>()
                   .Where(x => x.Kind == "plant")
                   .Select(x => this.Plants.FirstOrDefault(y => y.Id == x.AggregateId))
                   .Where(x => x != null)
                   .Subscribe(x =>
                   {
                       if (_Plants.Count == 1)
                       {
                           //var p =
                           //shouldAddMultiselectButtonSubscription.Dispose();
                           //shouldAddMultiselectButtonSubscription = p.IsEmptyChanged.StartWith(true).Subscribe(y =>
                           //{
                           //    currentTileModeDefaultButtons = !y ? TileModeDefaultButtonsWithSelection : TileModeDefaultButtons;
                           //    if (!IsPlantSelectionEnabled)
                           //    {
                           //        this.AppBarButtons = currentTileModeDefaultButtons;
                           //    }
                           //});

                           Plants = new ReactiveList<IPlantViewModel>()
                           {
                               ChangeTrackingEnabled = true
                           };

                           //Selecte
                       }
                       else
                           _Plants.Remove(x);
                       if (!MultiDeleteList.Contains(x) && SelfDeleteList.Contains(x))
                           this.NavigateBack();
                       //deleteSubscription.Dispose();
                   }));


                //this.SettingsF.ObserveOn(RxApp.TaskpoolScheduler).Subscribe(x => this.SettingsViewModel = x);
                //this.AddPlantViewModelF.ObserveOn(RxApp.TaskpoolScheduler).Subscribe(x => this.AddPlantViewModel = x);

                this.SettingsViewModel = SettingsF();
                this.AddPlantViewModel = AddPlantViewModelF();

            }


        }


        private ISettingsViewModel _SettingsViewModel;
        public ISettingsViewModel SettingsViewModel
        {
            get
            {
                return _SettingsViewModel;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SettingsViewModel, value);
            }
        }


        private IAddEditPlantViewModel _AddPlantViewModel;
        public IAddEditPlantViewModel AddPlantViewModel
        {
            get
            {
                return _AddPlantViewModel;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _AddPlantViewModel, value);
            }
        }



        private ButtonViewModel _AddPlantButton;
        public IButtonViewModel AddPlantButton
        {
            get
            {
                if (_AddPlantButton == null)
                    _AddPlantButton = new ButtonViewModel(null)
                    {
                        Text = "add",
                        IconType = IconType.ADD,
                        Command = TryAddPlantCommand
                    };
                return _AddPlantButton;
            }
        }


        private ButtonViewModel _SelectPlantsButton;
        public IButtonViewModel SelectPlantsButton
        {
            get
            {
                if (_SelectPlantsButton == null)
                    _SelectPlantsButton = new ButtonViewModel(App)
                    {
                        Text = "select",
                        IconType = IconType.CHECK_LIST,
                        Command = Observable.Return(true).ToCommandWithSubscription(x => this.IsPlantSelectionEnabled = !this.IsPlantSelectionEnabled)
                    };
                return _SelectPlantsButton;
            }
        }

        private ReactiveCommand DeletePlantsButtonCommand;
        private ReactiveCommand WaterPlantsButtonCommand;

        private ButtonViewModel _DeletePlantsButton;
        public IButtonViewModel DeletePlantsButton
        {
            get
            {
                if (_DeletePlantsButton == null)
                    _DeletePlantsButton = new ButtonViewModel(App)
                    {
                        Text = "delete",
                        IconType = IconType.DELETE,
                        Command = DeletePlantsButtonCommand
                    };
                return _DeletePlantsButton;
            }
        }

        private ButtonViewModel _WaterPlantsButton;
        public IButtonViewModel WaterPlantsButton
        {
            get
            {
                if (_WaterPlantsButton == null)
                    _WaterPlantsButton = new ButtonViewModel(App)
                    {
                        Text = "water",
                        IconType = IconType.WATER,
                        Command = WaterPlantsButtonCommand

                    };
                return _WaterPlantsButton;
            }
        }

        private ButtonViewModel _SettingsButton;
        public IButtonViewModel SettingsButton
        {
            get
            {
                if (_SettingsButton == null)
                    _SettingsButton = new ButtonViewModel(App)
                    {
                        Text = "Settings",
                        IconType = IconType.SETTINGS,
                        Command = Observable.Return(true).ToCommandWithSubscription((_) =>
                        {
                            //var svm = (ISettingsViewModel)App.Resolver.GetService(typeof(ISettingsViewModel));
                            this.WhenAnyValue(x => x.SettingsViewModel).Take(1).Subscribe(x =>
                            {
                                x.Plants = this.Plants;
                                this.Navigate(x);
                            });
                            //this.Navigate(svm);
                        })
                    };
                return _SettingsButton;
            }
        }

        //protected ReactiveList<IButtonViewModel> TileModeDefaultButtons;
        //protected ReactiveList<IButtonViewModel> TileModeMultiCommandButtons;
        //protected ReactiveList<IButtonViewModel> TileModeAppBarButtons;


        protected IReadOnlyReactiveList<IButtonViewModel> __AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return __AppBarButtons;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref __AppBarButtons, value);
            }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        protected ApplicationBarMode _AppBarMode = ApplicationBarMode.MINIMIZED;
        public ApplicationBarMode AppBarMode
        {
            get { return _AppBarMode; }
            set { this.RaiseAndSetIfChanged(ref _AppBarMode, value); }
        }

        private bool _AppBarIsVisible = true;
        public bool AppBarIsVisible
        {
            get
            {
                return _AppBarIsVisible;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _AppBarIsVisible, value);
            }
        }


        protected bool _IsPlantSelectionEnabled = false;
        public bool IsPlantSelectionEnabled
        {
            get { return _IsPlantSelectionEnabled; }
            set { this.RaiseAndSetIfChanged(ref _IsPlantSelectionEnabled, value); }
        }

        protected ReactiveList<IMenuItemViewModel> _AppBarMenuItems;
        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get
            {
                if (_AppBarMenuItems == null)
                    _AppBarMenuItems = new ReactiveList<IMenuItemViewModel>()
                    {
                        //new MenuItemViewModel(null){
                        //    Text = "settings"
                        //}
                    };
                return _AppBarMenuItems;
            }
        }


        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.Portrait; }
        }


        public override void Dispose()
        {
            base.Dispose();

            // allows for GC of plants in case
            // garden is not GC:d
            foreach (var p in Plants)
            {
                p.Dispose();
            }
            _Plants.Clear();
        }


    }




}