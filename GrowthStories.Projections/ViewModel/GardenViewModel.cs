
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
        public IPlantViewModel SelectedItem { get; set; }

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

        private IYAxisShitViewModel CurrentChartViewModel;


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
                app.WhenAny(x => x.User, x => x.GetValue()).Where(x => x != null).Take(1).Subscribe(x => this.Init(x));
            }



            //this.Id = iid;
            this.TileModeAppBarButtons.Add(this.AddPlantButton);
            this.AppBarButtons = this.TileModeAppBarButtons;



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
                    this.SelectedItem = x;
                    App.Router.Navigate.Execute(this);
                });

            //IObservable<long> timer = Observable.Interval(TimeSpan.FromSeconds(3));

            this.MultiWateringCommand = new ReactiveCommand();
            this.MultiWateringCommand.RegisterAsync(x => Observable.Create<bool>(obs =>
            {
                var timer = Observable.Timer(TimeSpan.FromSeconds(3));
                timer.Subscribe(_ => obs.OnNext(true), () => obs.OnCompleted());

                return Disposable.Empty;
            }));
            this.MultiDeleteCommand = new ReactiveCommand();
            this.MultiDeleteCommand.RegisterAsync(x => Observable.Create<bool>(obs =>
            {
                var timer = Observable.Timer(TimeSpan.FromSeconds(3));
                timer.Subscribe(_ => obs.OnNext(true), () => obs.OnCompleted());

                return Disposable.Empty;
            }));

            this.IsNotInProgress = MultiWateringCommand.CanExecuteObservable.CombineLatest(MultiDeleteCommand.CanExecuteObservable, (b1, b2) => b1 && b2).DistinctUntilChanged();

            //this.App.WhenAny(x => x.Orientation, x => x.GetValue())
            //    .CombineLatest(this.App.Router.CurrentViewModel.Where(x => x == this), (x, cvm) => ((x & PageOrientation.Landscape) == PageOrientation.Landscape))
            //    .Where(x => x == true)
            //    .DistinctUntilChanged()
            //    .Subscribe(_ =>
            //    {
            //        this.CurrentChartViewModel = App.YAxisShitViewModelFactory(this.SelectedItem);
            //        App.Router.Navigate.Execute(this.CurrentChartViewModel);
            //    });

            this.App.WhenAny(x => x.Orientation, x => x.GetValue())
                .Where(x => (x & PageOrientation.Landscape) == PageOrientation.Landscape && App.Router.GetCurrentViewModel() == this)
                .Subscribe(_ =>
                {
                    this.CurrentChartViewModel = App.YAxisShitViewModelFactory(this.SelectedItem);
                    App.Router.Navigate.Execute(this.CurrentChartViewModel);
                });

            this.App.WhenAny(x => x.Orientation, x => x.GetValue())
                .Where(x => (x & PageOrientation.Portrait) == PageOrientation.Portrait && App.Router.GetCurrentViewModel() == this.CurrentChartViewModel)
                .Subscribe(_ =>
                {
                    App.Router.Navigate.Execute(this);
                });


            App.Router.CurrentViewModel.Subscribe(x =>
            {
                if (x == this)
                {
                    this.IsInPivotMode = true;
                }
                else
                {
                    this.IsInPivotMode = false;
                }
            });

            this.WhenAny(x => x.IsInPivotMode, x => x.GetValue()).Subscribe(x =>
            {
                if (x == true && this.SelectedItem != null)
                    this.AppBarButtons = this.SelectedItem.AppBarButtons;
                else
                    this.AppBarButtons = this.TileModeAppBarButtons;
            });

        }

        protected bool _IsInPivotMode = false;
        public bool IsInPivotMode
        {
            get { return _IsInPivotMode; }
            set { this.RaiseAndSetIfChanged(ref _IsInPivotMode, value); }
        }

        private void Init(IAuthUser state)
        {
            this.User = state;
            this.Username = state.Username;

            //this.State = state.Garden;

            this.PlantTitle = string.Format("{0}'s garden", User.Username).ToUpper();
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

        public bool AppBarIsVisible
        {
            get { return true; }
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
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }
    }

    public class NotificationsViewModel : RoutableViewModel, INotificationsViewModel
    {
        public NotificationsViewModel(IGSAppViewModel app)
            : base(app)
        {

            this._AppBarButtons.Add(
            new ButtonViewModel(null)
                    {
                        Text = "add",
                        IconType = IconType.ADD,
                        Command = this.HostScreen.Router.NavigateCommandFor<IAddEditPlantViewModel>()
                    });

        }
        protected ReactiveList<IButtonViewModel> _AppBarButtons = new ReactiveList<IButtonViewModel>();
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get { return _AppBarButtons; }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.MINIMIZED; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }
    }


}