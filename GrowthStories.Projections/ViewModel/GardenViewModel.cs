
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

namespace Growthstories.UI.ViewModel
{


    public class GardenViewModel : RoutableViewModel, IGardenViewModel
    {


        protected ReactiveList<IPlantViewModel> _Plants;
        public IReadOnlyReactiveList<IPlantViewModel> Plants
        {
            get
            {
                if (_Plants == null)
                {
                    _Plants = new ReactiveList<IPlantViewModel>();
                    App.CurrentPlants(this.UserState).Concat(App.FuturePlants(this.UserState)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
                    {
                        _Plants.Add(x);
                    });
                    //_Plants.IsEmptyChanged.Where(x => x == false).Subscribe(_ => AppBarButtons.Add(SelectPlantsButton));
                    //_Plants.IsEmptyChanged.Where(x => x == true).Subscribe(_ => AppBarButtons.Remove(SelectPlantsButton));

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
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons { get; protected set; }

        public IReactiveCommand SelectedPlantsChangedCommand { get; protected set; }
        public IReactiveCommand ShowDetailsCommand { get; protected set; }
        public IReactiveCommand GetPlantCommand { get; protected set; }

        private IYAxisShitViewModel CurrentChartViewModel;


        //public PlantProjection PlantProjection { get; private set; }
        // public GardenState State { get; protected set; }
        public IAuthUser UserState { get; protected set; }

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

            if (state == null)
                throw new ArgumentNullException("UserState must be given.");

            this.UserState = state;
            this.Username = state.Username;

            //this.State = state.Garden;

            this.PlantTitle = string.Format("{0}'s garden", UserState.Username).ToUpper();

            //this.Id = iid;
            this.AppBarButtons = new ReactiveList<IButtonViewModel>();
            //this.AppBarButtons.Add(this.AddPlantButton);




            //this._SelectedPlants.IsEmptyChanged.Where(x => x == false).Subscribe(_ => AppBarButtons.Add(DeletePlantsButton));
            //this._SelectedPlants.IsEmptyChanged.Where(x => x == true).Subscribe(_ => AppBarButtons.Remove(DeletePlantsButton));


            SelectedPlantsChangedCommand = new ReactiveCommand();
            SelectedPlantsChangedCommand.Subscribe(p =>
            {
                _SelectedPlants.Clear();
                _SelectedPlants.AddRange(((IList)p).Cast<IPlantViewModel>());
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

        }



        private ButtonViewModel _AddPlantButton;
        public ButtonViewModel AddPlantButton
        {
            get
            {
                if (_AddPlantButton == null)
                    _AddPlantButton = new ButtonViewModel(null)
                    {
                        Text = "add",
                        IconUri = App.IconUri[IconType.ADD],
                        Command = Observable.Return(true).ToCommandWithSubscription(_ => this.Navigate(this.AddPlantViewModel))
                    };
                return _AddPlantButton;
            }
        }


        private ButtonViewModel _SelectPlantsButton;
        public ButtonViewModel SelectPlantsButton
        {
            get
            {
                if (_SelectPlantsButton == null)
                    _SelectPlantsButton = new ButtonViewModel(App)
                    {
                        Text = "select",
                        IconUri = App.IconUri[IconType.CHECK_LIST]
                        //Command = new ReactiveCommand(() => this.IsPlantSelectionEnabled = true)
                    };
                return _SelectPlantsButton;
            }
        }

        private ButtonViewModel _DeletePlantsButton;

        public ButtonViewModel DeletePlantsButton
        {
            get
            {
                if (_DeletePlantsButton == null)
                    _DeletePlantsButton = new ButtonViewModel(App)
                    {
                        Text = "delete",
                        IconUri = App.IconUri[IconType.DELETE]
                        //Command = new ReactiveCommand(() => { })
                    };
                return _DeletePlantsButton;
            }
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

        public bool IsPlantSelectionEnabled
        {
            get { return false; }
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

        protected IRoutableViewModel _AddPlantViewModel;
        public IRoutableViewModel AddPlantViewModel
        {
            get
            {
                return _AddPlantViewModel ?? (_AddPlantViewModel = App.AddPlantViewModelFactory(null));
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
                        IconUri = App.IconUri[IconType.ADD],
                        Command = this.HostScreen.Router.NavigateCommandFor<IAddPlantViewModel>()
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