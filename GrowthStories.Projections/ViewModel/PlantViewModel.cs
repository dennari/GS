
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
        Guid UserId { get; }
        string Name { get; }
        string Species { get; }
        ReactiveCommand PinCommand { get; }
        ReactiveCommand ScrollCommand { get; }
        Photo ProfilepictureData { get; }
    }

    public class PlantViewModel : RoutableViewModel, IPlantViewModel
    {


        protected ReactiveList<IPlantActionViewModel> _Actions;
        public ReactiveList<IPlantActionViewModel> Actions
        {
            get
            {
                if (_Actions == null)
                {
                    _Actions = new ReactiveList<IPlantActionViewModel>();
                    App.PlantActionViewModelFactory(this.State).Subscribe(x => this.Actions.Add(x));

                }
                return _Actions;
            }
        }



        public ReactiveCommand ShareCommand { get; protected set; }
        public ReactiveCommand DeleteCommand { get; protected set; }
        public ReactiveCommand PinCommand { get; protected set; }
        public ReactiveCommand ScrollCommand { get; protected set; }
        public ReactiveCommand FlickCommand { get; protected set; }

        public Guid Id { get { return State.Id; } }
        public Guid UserId { get { return State.UserId; } }


        protected PlantState State { get; set; }
        public IGardenViewModel Garden { get; protected set; }



        public PlantViewModel()
            : base(null)
        {

        }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PlantViewModel(PlantState state, IGardenViewModel garden, IGSApp app)
            : base(app)
        {
            //this.ActionProjection = actionProjection;
            //this.ActionProjection.EventHandled += this.ActionHandled;
            //this.Actions = new ObservableCollection<ActionBase>();
            if (state == null)
                throw new ArgumentNullException("PlantState has to be given in PlantViewModel");
            this.State = state;
            this.Garden = garden;



            this.ShareCommand = new ReactiveCommand();
            this.DeleteCommand = new ReactiveCommand();
            this.PinCommand = new ReactiveCommand();
            this.ScrollCommand = new ReactiveCommand();
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
            this.ScrollCommand = new ReactiveCommand();


            //this.GetActionsCommand = new ReactiveCommand();

            //this.GetActionsPipe = this.GetActionsCommand
            //    .RegisterAsyncFunction(this.App.Plant, RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);



            // THINK ABOUT NOT HAVING TO DO THE ROUNDTRIP!!!
            this.ListenTo<PlantActionCreated>()
                .Where(x => x.PlantId == this.State.Id)
                .Select(x => App.PlantActionViewModelFactory(this.State, x.AggregateId))
                .Switch()
                .Subscribe(x =>
                {
                    Actions.Insert(0, x);
                    ScrollCommand.Execute(x);
                });

            this.ListenTo<PlantActionPropertySet>()
                .Select(x => Tuple.Create(x, this.Actions.FirstOrDefault(y => y.PlantActionId == x.AggregateId)))
                .Where(x => x.Item2 != null)
                .Subscribe(x =>
                {
                    x.Item2.SetProperty(x.Item1);
                    ScrollCommand.Execute(x.Item2);
                });

            this.ListenTo<NameSet>(this.State.Id).Select(x => x.Name)
                .ToProperty(this, x => x.Name, out this._Name, state.Name);

            this.ListenTo<ProfilepictureSet>(this.State.Id).Select(x => x.Profilepicture)
                .ToProperty(this, x => x.ProfilepictureData, out this._ProfilepictureData, state.Profilepicture);

            this.ListenTo<SpeciesSet>(this.State.Id).Select(x => x.Species)
               .ToProperty(this, x => x.Species, out this._Species, state.Species);


            this.App.WhenAny(x => x.Orientation, x => x.GetValue())
                .CombineLatest(this.App.Router.CurrentViewModel.Where(x => x == this), (x, cvm) => ((x & PageOrientation.Landscape) == PageOrientation.Landscape))
                .Where(x => x == true)
                .Subscribe(_ => App.Router.Navigate.Execute(new YAxisShitViewModel(state, app)));



        }

        protected ObservableAsPropertyHelper<string> _Name;
        public string Name
        {
            get
            {
                return _Name.Value;
            }
        }

        protected ObservableAsPropertyHelper<Photo> _ProfilepictureData;
        public Photo ProfilepictureData
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
                            Command = Observable.Return(true)
                                .ToCommandWithSubscription(x => this.Navigate(this.AddWaterViewModel))
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "photograph",
                            IconUri = App.IconUri[IconType.PHOTO],
                            Command = Observable.Return(true)
                                .ToCommandWithSubscription(x => this.Navigate(this.AddPhotographViewModel))
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "comment",
                            IconUri = App.IconUri[IconType.NOTE],
                            Command = Observable.Return(true)
                                .ToCommandWithSubscription(x => this.Navigate(this.AddCommentViewModel))
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
                            Command = Observable.Return(true)
                               .ToCommandWithSubscription(x => this.Navigate(this.AddMeasureViewModel))
                        },
                        new MenuItemViewModel(null)
                        {
                            Text = "nourish",
                            Command = Observable.Return(true)
                                .ToCommandWithSubscription(x => this.Navigate(this.AddFertilizeViewModel))
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

        #region ADDVIEWMODELS
        protected IPlantActionViewModel _AddWaterViewModel;
        public IPlantActionViewModel AddWaterViewModel
        {
            get
            {
                if (_AddWaterViewModel == null)
                {
                    _AddWaterViewModel = App.PlantActionViewModelFactory<IPlantWaterViewModel>();
                    _AddWaterViewModel.AddCommand.Subscribe(_ =>
                    {
                        //this.SendCommand(new Water(this.UserId, this.Id, _AddWaterViewModel.Note), true);
                        this.SendCommand(
                            new CreatePlantAction(
                                Guid.NewGuid(),
                                this.UserId,
                                this.Id,
                                PlantActionType.WATERED,
                                _AddWaterViewModel.Note
                            ),
                        true);
                    });
                }
                return _AddWaterViewModel;
            }
        }

        protected IPlantActionViewModel _AddCommentViewModel;
        public IPlantActionViewModel AddCommentViewModel
        {
            get
            {
                if (_AddCommentViewModel == null)
                {
                    _AddCommentViewModel = App.PlantActionViewModelFactory<IPlantCommentViewModel>();
                    _AddCommentViewModel.AddCommand.Subscribe(_ =>
                    {
                        this.SendCommand(
                            new CreatePlantAction(
                                Guid.NewGuid(),
                                this.UserId,
                                this.Id,
                                PlantActionType.COMMENTED,
                                _AddCommentViewModel.Note
                            ),
                        true);
                    });
                }
                return _AddCommentViewModel;
            }
        }

        protected IPlantActionViewModel _AddFertilizeViewModel;
        public IPlantActionViewModel AddFertilizeViewModel
        {
            get
            {
                if (_AddFertilizeViewModel == null)
                {
                    _AddFertilizeViewModel = App.PlantActionViewModelFactory<IPlantFertilizeViewModel>();
                    _AddFertilizeViewModel.AddCommand.Subscribe(_ =>
                    {
                        this.SendCommand(
                            new CreatePlantAction(
                                Guid.NewGuid(),
                                this.UserId,
                                this.Id,
                                PlantActionType.WATERED,
                                _AddFertilizeViewModel.Note
                            ),
                        true);
                    });
                }
                return _AddFertilizeViewModel;
            }
        }

        protected IPlantPhotographViewModel _AddPhotographViewModel;
        public IPlantPhotographViewModel AddPhotographViewModel
        {
            get
            {
                if (_AddPhotographViewModel == null)
                {
                    var vm = (IPlantPhotographViewModel)App.PlantActionViewModelFactory<IPlantPhotographViewModel>();
                    vm.AddCommand.Subscribe(_ =>
                    {
                        this.SendCommand(
                            new CreatePlantAction(
                                Guid.NewGuid(),
                                this.UserId,
                                this.Id,
                                PlantActionType.PHOTOGRAPHED,
                                vm.Note
                            )
                            {
                                Photo = vm.PhotoData
                            },
                        true);
                    });
                    _AddPhotographViewModel = vm;
                }
                return _AddPhotographViewModel;
            }
        }

        protected IPlantMeasureViewModel _AddMeasureViewModel;
        public IPlantMeasureViewModel AddMeasureViewModel
        {
            get
            {
                if (_AddMeasureViewModel == null)
                {
                    var vm = (IPlantMeasureViewModel)App.PlantActionViewModelFactory<IPlantMeasureViewModel>();
                    vm.AddCommand.Subscribe(_ =>
                    {
                        this.SendCommand(
                            new CreatePlantAction(
                                Guid.NewGuid(),
                                this.UserId,
                                this.Id,
                                PlantActionType.MEASURED,
                                vm.Note
                            )
                            {
                                MeasurementType = vm.Series.Type,
                                Value = vm.Value.Value
                            },
                        true);
                    });
                    _AddMeasureViewModel = vm;
                }
                return _AddMeasureViewModel;
            }
        }

        #endregion

        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }


    }


    public class PlantViewModelDesign
    {

        public PlantState State;

        public PlantActionViewModel Test
        {
            get
            {
                return this.Actions.First();
            }
        }

        public List<PlantActionViewModel> Actions
        {
            get
            {
                return new List<PlantActionViewModel>()
                {

                    //new WaterViewModel(new Watered(State.UserId,State.Id,"Watered"),null),
                    //new FertilizeViewModel(new Fertilized(State.UserId,State.Id,"Fertilized"),null),
                    //new CommentViewModel(new Commented(State.UserId,State.Id,"Commented"),null),
                    //new PhotographViewModel(new Photographed(State.UserId, State.Id, "My baby!", new Uri("ms-appx:///TestData/517e100d782a828894.jpg")),null)


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