
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
        Photo Photo { get; }
        PlantState State { get; }
        ReactiveList<IPlantActionViewModel> Actions { get; }
    }

    public class PlantViewModel : RoutableViewModel, IPlantViewModel
    {




        public IObservable<IPlantActionViewModel> PlantActionStream { get; protected set; }

        public ReactiveCommand ShareCommand { get; protected set; }
        public ReactiveCommand DeleteCommand { get; protected set; }
        public ReactiveCommand PinCommand { get; protected set; }
        public ReactiveCommand ScrollCommand { get; protected set; }
        public ReactiveCommand FlickCommand { get; protected set; }
        public ReactiveCommand PlantActionDetailsCommand { get; protected set; }

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


            //this.FlickCommand.Subscribe(x =>
            //{
            //    var xx = x as Tuple<double, double>;
            //    if (garden != null && xx != null && Math.Abs(xx.Item1) > Math.Abs(xx.Item2))
            //    {
            //        var myIdx = garden.Plants.IndexOf(this);
            //        if (myIdx < garden.Plants.Count - 1 && xx.Item1 < 0)
            //            App.Router.Navigate.Execute(garden.Plants[myIdx + 1]);
            //        if (myIdx > 0 && xx.Item1 > 0)
            //            App.Router.Navigate.Execute(garden.Plants[myIdx - 1]);

            //    }
            //});
            this.ScrollCommand = new ReactiveCommand();


            //this.GetActionsCommand = new ReactiveCommand();

            //this.GetActionsPipe = this.GetActionsCommand
            //    .RegisterAsyncFunction(this.App.Plant, RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);



            // THINK ABOUT NOT HAVING TO DO THE ROUNDTRIP!!!



            //this.PlantActionStream = ;

            //this.ListenTo<PlantActionPropertySet>()
            //    .Select(x => Tuple.Create(x, this.Actions.FirstOrDefault(y => y.PlantActionId == x.AggregateId)))
            //    .Where(x => x.Item2 != null)
            //    .Subscribe(x =>
            //    {
            //        x.Item2.SetProperty(x.Item1);
            //        ScrollCommand.Execute(x.Item2);
            //    });

            this.ListenTo<NameSet>(this.State.Id).Select(x => x.Name)
                .ToProperty(this, x => x.Name, out this._Name, state.Name);

            this.ListenTo<ProfilepictureSet>(this.State.Id).Select(x => x.Profilepicture)
                .ToProperty(this, x => x.Photo, out this._ProfilepictureData, state.Profilepicture);

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

        protected ReactiveList<IPlantActionViewModel> _Actions;
        public ReactiveList<IPlantActionViewModel> Actions
        {
            get
            {
                if (_Actions == null)
                {
                    _Actions = new ReactiveList<IPlantActionViewModel>();
                    App.CurrentPlantActions(this.State).Concat(App.FuturePlantActions(this.State)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
                    {
                        Actions.Insert(0, x);
                        ScrollCommand.Execute(x);
                    });
                    //App.FuturePlantActions(this.State).Subscribe(x =>
                    //{
                    //    Actions.Insert(0, x);
                    //    ScrollCommand.Execute(x);
                    //});

                }
                return _Actions;
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


    }



}