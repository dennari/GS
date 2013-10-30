
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
    public interface IGardenViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar, IHasMenuItems
    {
        Guid Id { get; }
        //GardenState State { get; }
        IAuthUser UserState { get; }
        ReactiveList<IPlantViewModel> Plants { get; }
        string Username { get; }
    }

    public interface INotificationsViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }



    public class GardenViewModel : RoutableViewModel, IGardenViewModel
    {


        protected ReactiveList<IPlantViewModel> _Plants;
        public ReactiveList<IPlantViewModel> Plants
        {
            get
            {
                if (_Plants == null)
                {
                    _Plants = new ReactiveList<IPlantViewModel>();
                    App.CurrentPlants(this.UserState).Concat(App.FuturePlants(this.UserState)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
                    {
                        Plants.Add(x);
                    });
                    Plants.IsEmptyChanged.Where(x => x == false).Subscribe(_ => AppBarButtons.Add(SelectPlantsButton));
                    Plants.IsEmptyChanged.Where(x => x == true).Subscribe(_ => AppBarButtons.Remove(SelectPlantsButton));

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

        public ReactiveList<IPlantViewModel> SelectedPlants { get; protected set; }
        public ReactiveList<ButtonViewModel> AppBarButtons { get; protected set; }

        public IReactiveCommand SelectedPlantsChangedCommand { get; protected set; }
        public IReactiveCommand ShowDetailsCommand { get; protected set; }
        public IReactiveCommand GetPlantCommand { get; protected set; }


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
            //this.State = state.Garden;

            this.PlantTitle = string.Format("{0}'s garden", UserState.Username).ToUpper();

            //this.Id = iid;
            this.SelectedPlants = new ReactiveList<IPlantViewModel>();
            this.AppBarButtons = new ReactiveList<ButtonViewModel>();
            this.AppBarButtons.Add(this.AddPlantButton);




            this.SelectedPlants.IsEmptyChanged.Where(x => x == false).Subscribe(_ => AppBarButtons.Add(DeletePlantsButton));
            this.SelectedPlants.IsEmptyChanged.Where(x => x == true).Subscribe(_ => AppBarButtons.Remove(DeletePlantsButton));


            SelectedPlantsChangedCommand = new ReactiveCommand();
            SelectedPlantsChangedCommand.Subscribe(p =>
            {
                SelectedPlants.Clear();
                SelectedPlants.AddRange(((IList)p).Cast<IPlantViewModel>());
            });


            //this.GetPlantCommand = new ReactiveCommand();
            //this.GetPlantPipe = this.GetPlantCommand
            //    .RegisterAsyncFunction((id) => pvmFactory((Guid)id, this), RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);



            this.ShowDetailsCommand = new ReactiveCommand();
            this.ShowDetailsCommand
                .OfType<IPlantViewModel>()
                .Subscribe(x =>
                {
                    var pivot = new GardenPivotViewModel(x, Plants, state, App);
                    App.Router.Navigate.Execute(pivot);
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

        protected ReactiveList<MenuItemViewModel> _AppBarMenuItems;
        public ReactiveList<MenuItemViewModel> AppBarMenuItems
        {
            get
            {
                if (_AppBarMenuItems == null)
                    _AppBarMenuItems = new ReactiveList<MenuItemViewModel>()
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

    }

    public class NotificationsViewModel : RoutableViewModel, INotificationsViewModel
    {
        public NotificationsViewModel(IGSAppViewModel app)
            : base(app)
        {

            this.AppBarButtons.Add(
            new ButtonViewModel(null)
                    {
                        Text = "add",
                        IconUri = App.IconUri[IconType.ADD],
                        Command = this.HostScreen.Router.NavigateCommandFor<IAddPlantViewModel>()
                    });

        }
        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get { return _AppBarButtons ?? (_AppBarButtons = new ReactiveList<ButtonViewModel>()); }
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

    public class FriendsViewModel : RoutableViewModel, IHasAppBarButtons, IControlsAppBar
    {


        public ReactiveCommand FriendSelected { get; protected set; }

        protected ReactiveList<IGardenViewModel> _Friends;
        public ReactiveList<IGardenViewModel> Friends
        {
            get
            {
                if (_Friends == null)
                {
                    _Friends = new ReactiveList<IGardenViewModel>();
                    App.CurrentGardens().Concat(App.FutureGardens()).Where(x => x.UserState.Id != App.Context.CurrentUser.Id).DistinctUntilChanged().Subscribe(x =>
                    {
                        Friends.Add(x);
                    });
                    //App.FuturePlantActions(this.State).Subscribe(x =>
                    //{
                    //    Actions.Insert(0, x);
                    //    ScrollCommand.Execute(x);
                    //});

                }
                return _Friends;
            }
        }



        public FriendsViewModel(IGSAppViewModel app)
            : base(app)
        {

            var listUsersCommand = new ReactiveCommand();
            listUsersCommand.Subscribe(x =>
            {
                var lvm = App.Resolver.GetService<ListUsersViewModel>();
                App.Router.Navigate.Execute(lvm);
            });

            this.AppBarButtons.Add(
            new ButtonViewModel(null)
            {
                Text = "add",
                IconUri = App.IconUri[IconType.ADD],
                Command = listUsersCommand
            });

            this.FriendSelected = new ReactiveCommand();
            this.FriendSelected.OfType<IGardenViewModel>().Subscribe(x =>
            {
                App.Router.Navigate.Execute(x);
            });

        }
        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get { return _AppBarButtons ?? (_AppBarButtons = new ReactiveList<ButtonViewModel>()); }
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