
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
        ReactiveList<IPlantViewModel> Plants { get; }
    }

    public interface INotificationsViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }

    public interface IFriendsViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar
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
                    LoadPlants();
                    //a();

                }
                return _Plants;
            }
        }

        public ReactiveList<IPlantViewModel> SelectedPlants { get; protected set; }
        public ReactiveList<ButtonViewModel> AppBarButtons { get; protected set; }

        public IReactiveCommand SelectedPlantsChangedCommand { get; protected set; }
        public IReactiveCommand ShowDetailsCommand { get; protected set; }
        public IReactiveCommand GetPlantCommand { get; protected set; }


        public PlantProjection PlantProjection { get; private set; }
        private readonly GardenState State;

        public Guid Id { get { return State.Id; } }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public GardenViewModel(
            GardenState state,
            Func<Guid, IGardenViewModel, IPlantViewModel> pvmFactory,
            IGSApp app)
            : base(app)
        {

            this.State = state;

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


            this.GetPlantCommand = new ReactiveCommand();
            this.GetPlantPipe = this.GetPlantCommand
                .RegisterAsyncFunction((id) => pvmFactory((Guid)id, this), RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            this.GetPlantPipe.Subscribe(x => this.Plants.Add(x));

            this.ShowDetailsCommand = new ReactiveCommand();
            this.ShowDetailsCommand.Subscribe(x => App.Router.Navigate.Execute(x));

            App.Bus.Listen<IEvent>().OfType<PlantAdded>()
                .Where(x =>
                {
                    return x.EntityId == this.State.Id;
                })
                .Subscribe(x =>
                {
                    this.GetPlantCommand.Execute(x.PlantId);
                });

        }

        protected void LoadPlants()
        {

            _Plants.IsEmptyChanged.Where(x => x == false).Subscribe(_ => AppBarButtons.Add(SelectPlantsButton));
            _Plants.IsEmptyChanged.Where(x => x == true).Subscribe(_ => AppBarButtons.Remove(SelectPlantsButton));
            foreach (var id in this.State.PlantIds)
            {
                this.GetPlantCommand.Execute(id);
                //var vm = await this.GetPlantPipe.FirstAsync();
                //this.Plants.Add(vm);
            }
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
        private IObservable<IPlantViewModel> GetPlantPipe;

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
                    _AppBarMenuItems = new ReactiveList<MenuItemViewModel>(){
                        new MenuItemViewModel(null){
                            Text = "settings"
                        }
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
        public NotificationsViewModel(IGSApp app)
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

    public class FriendsViewModel : RoutableViewModel, IFriendsViewModel
    {
        public FriendsViewModel(IGSApp app)
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
}