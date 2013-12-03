
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Disposables;

namespace Growthstories.UI.ViewModel
{


    public class GardenViewModel : RoutableViewModel, IGardenViewModel
    {





        private void LoadPlants(IAuthUser u)
        {
            App.CurrentPlants(u).Concat(App.FuturePlants(u)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
            {
                _Plants.Add(x);
            });
        }

        protected ReactiveList<IPlantViewModel> _Plants;
        public IReadOnlyReactiveList<IPlantViewModel> Plants
        {
            get
            {
                if (_Plants == null)
                {
                    _Plants = new ReactiveList<IPlantViewModel>();
                    if (this.User != null)
                    {
                        LoadPlants(this.User);
                    }
                    else
                    {
                        this.WhenAny(x => x.User, x => x.GetValue()).Where(x => x != null).Take(1).Subscribe(x => LoadPlants(x));
                    }

                    _Plants.IsEmptyChanged.Where(x => x == false).Subscribe(_ => TileModeAppBarButtons.Add(SelectPlantsButton));
                    _Plants.IsEmptyChanged.Where(x => x == true).Subscribe(_ => TileModeAppBarButtons.Remove(SelectPlantsButton));

                    //App.FuturePlants(this.UserState)
                    //.Subscribe(x =>
                    //{
                    //    Plants.Add(x);
                    //});


                }
                return _Plants;
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

        protected ReactiveList<IButtonViewModel> TileModeAppBarButtons = new ReactiveList<IButtonViewModel>();
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

        public IReactiveCommand SelectedItemsChanged { get; protected set; }
        public IReactiveCommand ShowDetailsCommand { get; protected set; }
        public IReactiveCommand GetPlantCommand { get; protected set; }



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

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public GardenViewModel(
            IAuthUser state,
            IGSAppViewModel app)
            : base(app)
        {

            if (state != null)
                this.Init(state);
            else
            {

                app.WhenAnyValue(x => x.User)
                    .Where(x => x != null)
                    .Take(1)
                    .Subscribe(x => this.Init(x));
            }



            //this.Id = iid;



            this._SelectedPlants.IsEmptyChanged.Subscribe(x =>
            {
                if (x == false)
                {
                    AppBarMode = ApplicationBarMode.DEFAULT;
                    TileModeAppBarButtons.Remove(SelectPlantsButton);
                    TileModeAppBarButtons.Remove(AddPlantButton);
                    TileModeAppBarButtons.Add(DeletePlantsButton);
                    TileModeAppBarButtons.Add(WaterPlantsButton);

                }
                else
                {
                    AppBarMode = ApplicationBarMode.MINIMIZED;
                    TileModeAppBarButtons.Remove(WaterPlantsButton);
                    TileModeAppBarButtons.Remove(DeletePlantsButton);
                    TileModeAppBarButtons.Add(AddPlantButton);
                    if (_Plants != null && _Plants.Count > 0 && !TileModeAppBarButtons.Contains(SelectPlantsButton))
                        TileModeAppBarButtons.Add(SelectPlantsButton);
                }
            });


            SelectedItemsChanged = new ReactiveCommand();
            SelectedItemsChanged.Subscribe(p =>
            {
                var ar = p as Tuple<IList, IList>;
                if (ar != null)
                {
                    _SelectedPlants.AddRange(ar.Item1.Cast<IPlantViewModel>());
                    foreach (var plant in ar.Item2.Cast<IPlantViewModel>())
                        _SelectedPlants.Remove(plant);
                }
            });


            //this.GetPlantCommand = new ReactiveCommand();
            //this.GetPlantPipe = this.GetPlantCommand
            //    .RegisterAsyncFunction((id) => pvmFactory((Guid)id, this), RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);



            this.ShowDetailsCommand = new ReactiveCommand();
            this.ShowDetailsCommand
                .OfType<IPlantViewModel>()
                .Subscribe(x =>
                {
                    //var pivot = new GardenPivotViewModel(x, Plants, state, App);
                    //this.SelectedItem = x;
                    this.PivotVM.SelectedItem = x;
                    App.Router.Navigate.Execute(this.PivotVM);
                });

            //IObservable<long> timer = Observable.Interval(TimeSpan.FromSeconds(3));

            this.MultiWateringCommand = new ReactiveCommand();



            this.MultiWateringCommand.Subscribe((_) =>
            {
                if (this._SelectedPlants.Count == 0)
                    return;
                foreach (var x in _SelectedPlants)
                    x.WateringCommand.Execute(null);
            });
            this.MultiDeleteCommand = new ReactiveCommand();
            this.MultiDeleteCommand.Subscribe((_) =>
            {
                if (this._SelectedPlants.Count == 0)
                    return;
                foreach (var x in _SelectedPlants)
                    x.DeleteCommand.Execute(null);
            });

            this.IsNotInProgress = MultiWateringCommand.CanExecuteObservable.CombineLatest(MultiDeleteCommand.CanExecuteObservable, (b1, b2) => b1 && b2).DistinctUntilChanged();

            //     x => Observable.Create<bool>(obs =>
            //{
            //    var timer = Observable.Timer(TimeSpan.FromSeconds(3));
            //    timer.Subscribe(_ => obs.OnNext(true), () => obs.OnCompleted());

            //    return Disposable.Empty;
            // })

        }

        protected IGardenPivotViewModel _PivotVM;
        public IGardenPivotViewModel PivotVM
        {
            get
            {
                if (_PivotVM == null)
                {
                    _PivotVM = new GardenPivotViewModel(this);
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


        private void Init(IAuthUser user)
        {
            this.User = user;
            this.Username = user.Username;
            this.Id = user.GardenId;
            //this.State = state.Garden;

            this.PlantTitle = string.Format("{0}'s garden", User.Username).ToUpper();

            if (App.User == null || App.User.Id == null || user.Id == App.User.Id)
            {
                OwnGarden = true;
                this.TileModeAppBarButtons.Add(this.AddPlantButton);
                this.TileModeAppBarButtons.Add(this.SettingsButton);
                this.AppBarButtons = this.TileModeAppBarButtons;
                this.AppBarIsVisible = true;

            }
            else
            {
                OwnGarden = false;
                this.AppBarIsVisible = false;
                OwnGarden = user.Id == App.User.Id;
            }

            /*
            if (user.Id == App.User.Id)
            {
                this.TileModeAppBarButtons.Add(this.AddPlantButton);
                this.TileModeAppBarButtons.Add(this.SettingsButton);
                this.AppBarButtons = this.TileModeAppBarButtons;
                this.AppBarIsVisible = true;
            }
            else
            {
                this.AppBarIsVisible = false;
            }
            */
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
                        Command = App.Router.NavigateCommandFor<IAddEditPlantViewModel>()
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
                        Command = this.IsNotInProgress.ToCommandWithSubscription(x => this.MultiDeleteCommand.Execute(null))
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
                        Command = this.IsNotInProgress.ToCommandWithSubscription(x => this.MultiWateringCommand.Execute(null))

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
                        Command = Observable.Return(true).ToCommandWithSubscription((_) => this.Navigate(new SettingsViewModel(App)))

                    };
                return _SettingsButton;
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

        private bool _AppBarIsVisible = false;
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
    }




}