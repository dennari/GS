
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

    public interface IPlantViewModel : IGSRoutableViewModel, IHasAppBarButtons, IHasMenuItems, IControlsAppBar, IControlsPageOrientation
    {
        Guid Id { get; }
        string Name { get; }
        string ProfilepicturePath { get; }
    }

    public class PlantViewModel : RoutableViewModel, IPlantViewModel
    {

        PlantActionViewModel _Test;
        public PlantActionViewModel Test
        {
            get { return _Test; }
            protected set { this.RaiseAndSetIfChanged(ref _Test, value); }
        }


        protected ReactiveList<PlantActionViewModel> _Actions;
        public ReactiveList<PlantActionViewModel> Actions
        {
            get
            {
                if (_Actions == null)
                {
                    _Actions = new ReactiveList<PlantActionViewModel>();
                    //LoadActions();
                    //a();
                    this.GetActionsCommand.Execute(Tuple.Create(State.UserId, State.Id));

                }
                return _Actions;
            }
        }

        private ReactiveCommand GetActionsCommand;
        private IObservable<IEnumerable<PlantActionViewModel>> GetActionsPipe;

        public ReactiveCommand AddWaterCommand { get; protected set; }
        public ReactiveCommand AddFertilizerCommand { get; protected set; }
        public ReactiveCommand AddCommentCommand { get; protected set; }
        public ReactiveCommand AddPhotographCommand { get; protected set; }
        public ReactiveCommand ShareCommand { get; protected set; }
        public ReactiveCommand EditCommand { get; protected set; }
        public ReactiveCommand DeleteCommand { get; protected set; }
        public ReactiveCommand PinCommand { get; protected set; }
        public ReactiveCommand AddMeasurementCommand { get; protected set; }
        public ReactiveCommand FlickCommand { get; protected set; }



        public Guid Id { get { return State.Id; } }
        public string Name { get { return State.Name; } }
        public string ProfilepicturePath { get { return State.ProfilepicturePath; } }


        private readonly Func<object, IEnumerable<ActionBase>> ActionFactory;

        public PlantState State { get; protected set; }
        public IGardenViewModel Garden { get; protected set; }


        public PlantViewModel()
            : base(null)
        {

        }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PlantViewModel(PlantState state, IGardenViewModel garden, Func<object, IEnumerable<ActionBase>> actionFactory, IGSApp app)
            : base(app)
        {
            //this.ActionProjection = actionProjection;
            //this.ActionProjection.EventHandled += this.ActionHandled;
            //this.Actions = new ObservableCollection<ActionBase>();
            this.State = state;
            this.ActionFactory = actionFactory;
            this.Garden = garden;

            this.AddWaterCommand = new ReactiveCommand();
            this.AddWaterCommand.Subscribe(x => App.Router.Navigate.Execute(App.ActionViewModelFactory(typeof(AddWaterViewModel), state, app)));
            this.AddCommentCommand = new ReactiveCommand();
            this.AddCommentCommand.Subscribe(x => App.Router.Navigate.Execute(App.ActionViewModelFactory(typeof(AddCommentViewModel), state, app)));
            this.AddFertilizerCommand = new ReactiveCommand();
            this.AddFertilizerCommand.Subscribe(x => App.Router.Navigate.Execute(App.ActionViewModelFactory(typeof(AddFertilizerViewModel), state, app)));
            this.AddPhotographCommand = new ReactiveCommand();
            this.AddPhotographCommand.Subscribe(x => App.Router.Navigate.Execute(App.ActionViewModelFactory(typeof(AddPhotographViewModel), state, app)));
            this.AddMeasurementCommand = new ReactiveCommand();
            this.AddMeasurementCommand.Subscribe(x => App.Router.Navigate.Execute(App.ActionViewModelFactory(typeof(AddMeasurementViewModel), state, app)));
            this.ShareCommand = new ReactiveCommand();
            this.EditCommand = new ReactiveCommand();
            this.DeleteCommand = new ReactiveCommand();
            this.PinCommand = new ReactiveCommand();
            this.FlickCommand = new ReactiveCommand();
            this.FlickCommand.Subscribe(x =>
            {
                var xx = x as Tuple<double, double>;
                if (garden != null && xx != null && Math.Abs(xx.Item1) > Math.Abs(xx.Item2))
                {
                    var myIdx = garden.Plants.IndexOf(this);
                    if (myIdx < garden.Plants.Count - 1 && xx.Item1 < 0)
                        App.Router.Navigate.Execute(garden.Plants[myIdx + 1]);
                    if (myIdx > 0 && xx.Item1 > 0)
                        App.Router.Navigate.Execute(garden.Plants[myIdx - 1]);

                }
            });



            this.GetActionsCommand = new ReactiveCommand();
            this.GetActionsPipe = this.GetActionsCommand
                .RegisterAsyncFunction((id) => PlantActionViewModelFactory.GetVM(actionFactory(id), this.App), RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            this.GetActionsPipe.Subscribe(x => this._Actions.AddRange(x));

            App.Bus.Listen<IEvent>().OfType<ActionBase>()
                .Where(x => x.EntityId == this.State.UserId && x.PlantId == this.State.Id)
                .Select(x => PlantActionViewModelFactory.GetVM(x, App))
                .Subscribe(x =>
                {
                    Actions.Add(x);
                });

            this.App.WhenAny(x => x.Orientation, x => x.GetValue())
                .CombineLatest(this.App.Router.CurrentViewModel.Where(x => x == this), (x, cvm) => ((x & PageOrientation.Landscape) == PageOrientation.Landscape))
                .Where(x => x == true)
                .Subscribe(_ => App.Router.Navigate.Execute(new YAxisShitViewModel(state, app)));



        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        #region APPBAR
        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                    _AppBarButtons = new ReactiveList<ButtonViewModel>()
                    {
                        new ButtonViewModel(null)
                        {
                            Text = "water",
                            IconUri = App.IconUri[IconType.WATER],
                            Command = AddWaterCommand
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "photograph",
                            IconUri = App.IconUri[IconType.PHOTO],
                            Command = AddPhotographCommand
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "comment",
                            IconUri = App.IconUri[IconType.NOTE],
                            Command = AddCommentCommand
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

        protected ReactiveList<MenuItemViewModel> _AppBarMenuItems;
        public ReactiveList<MenuItemViewModel> AppBarMenuItems
        {
            get
            {
                if (_AppBarMenuItems == null)
                    _AppBarMenuItems = new ReactiveList<MenuItemViewModel>()
                    {
                        new MenuItemViewModel(null)
                        {
                            Text = "measure",
                            Command = AddMeasurementCommand
                        },
                        new MenuItemViewModel(null)
                        {
                            Text = "nourish",
                            Command = AddFertilizerCommand
                        },
                          new MenuItemViewModel(null)
                        {
                            Text = "edit",
                            Command = EditCommand
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


    }


    public class PlantViewModelDesign : PlantViewModel
    {

        public new PlantActionViewModel Test
        {
            get
            {
                return this.Actions.First();
            }
        }

        public new ReactiveList<PlantActionViewModel> Actions
        {
            get
            {
                return new ReactiveList<PlantActionViewModel>()
                {
                    new WaterViewModel(new Watered(State.UserId,State.Id,"Watered"),this.App),
                    new FertilizeViewModel(new Fertilized(State.UserId,State.Id,"Fertilized"),this.App),
                    new CommentViewModel(new Commented(State.UserId,State.Id,"Commented"),this.App),
                    new PhotographViewModel(new Photographed(State.UserId, State.Id, "My baby!", new Uri("ms-appx:///TestData/517e100d782a828894.jpg")),this.App)


                };
            }
        }

        public PlantViewModelDesign()
            : base()
        {
            this.State = new PlantState(new PlantCreated(Guid.NewGuid(), "Jare", Guid.NewGuid()));
        }
    }
}