
using System;
using System.Reactive.Linq;
using Growthstories.Core;
using ReactiveUI;
using Growthstories.Domain.Entities;

namespace Growthstories.UI.ViewModel
{


    public class GardenPivotViewModel : MultipageViewModel, IGardenPivotViewModel
    {

        protected IPlantViewModel _SelectedPlant;
        public IPlantViewModel SelectedPlant { get { return _SelectedPlant; } set { this.RaiseAndSetIfChanged(ref _SelectedPlant, value); } }

        // just to keep compiler happy
        public IReactiveCommand ShowDetailsCommand { get; private set; }


        private IGSViewModel _InnerViewModel;
        public IGSViewModel InnerViewModel
        {
            get
            {
                return _InnerViewModel;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _InnerViewModel, value);
            }
        }


        private readonly IGardenViewModel Vm;

        public GardenPivotViewModel(IGardenViewModel vm, IGSAppViewModel app)
            : base(app)
        {
            this.Vm = vm;


            // when changing pivot pages, change current plant
            this.WhenAny(x => x.SelectedPage, x => x.GetValue())
                .Where(x => x != null)
                .OfType<IPlantViewModel>()
                .Subscribe(x =>
                {
                    this.SelectedPlant = x;
                    this.Log().Info("SelectedPlant changed to {0}", x.Name);
                });

            vm.WhenAnyValue(x => x.Plants)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(x =>
                {
                    this.SelectedItem = null;
                })
                .ToProperty(this, x => x.Plants, out _Plants);

            // when orientation changes to landscape, show current plant's chart
            Observable.CombineLatest(
                this.App.WhenAnyValue(y => y.Orientation),
                this.App.Router.CurrentViewModel,
                (o, v) => Tuple.Create(o, v)
             ).Subscribe(xx =>
                {
                    //this.NavigateInterface = typeof(IYAxisShitViewModel);
                    var o = xx.Item1;
                    if ((o & PageOrientation.Landscape) == PageOrientation.Landscape && this.SelectedPlant != null)
                    {
                        this.SelectedPage = this.SelectedPlant.Chart;
                        this.InnerViewModel = this.SelectedPlant.Chart;
                    }
                    else
                    {
                        this.InnerViewModel = null;
                        this.SelectedPage = this.SelectedPlant;
                    }
                    //App.Router.Navigate.Execute(this.CurrentChartViewModel);
                });


            this.WateringCommand = new ReactiveCommand();
            this.WateringCommand.Subscribe(x =>
            {
                if (this.SelectedPlant != null)
                    this.SelectedPlant.WateringCommand.Execute(x);
            });
            this.PhotoCommand = new ReactiveCommand();
            this.PhotoCommand.Subscribe(x =>
            {
                if (this.SelectedPlant != null)
                    this.SelectedPlant.PhotoCommand.Execute(x);
            });
            this.NavigateToEmptyActionCommand = new ReactiveCommand();
            this.NavigateToEmptyActionCommand.Subscribe(x =>
            {
                if (this.SelectedPlant != null)
                    this.SelectedPlant.NavigateToEmptyActionCommand.Execute(x);
            });
            this.TryShareCommand = new ReactiveCommand();
            this.TryShareCommand.Subscribe(x =>
            {
                if (this.SelectedPlant != null)
                    this.SelectedPlant.TryShareCommand.Execute(x);
            });

            this.AppBarButtons = GetOwnerButtons();

        }

        public IReactiveCommand WateringCommand { get; protected set; }

        public IReactiveCommand PhotoCommand { get; protected set; }

        public IReactiveCommand NavigateToEmptyActionCommand { get; protected set; }


        public IReactiveCommand TryShareCommand { get; protected set; }




        private IPlantActionListViewModel _PlantActionList;
        protected IPlantActionListViewModel PlantActionList
        {
            get
            {
                if (_PlantActionList == null)
                {
                    _PlantActionList = new PlantActionListViewModel(null, App);
                }
                return _PlantActionList;
            }
        }

        public Guid Id
        {
            get { return Vm.Id; }
        }

        public Guid UserId
        {
            get { return Vm.UserId; }
        }

        public IReactiveCommand SelectedItemsChanged
        {
            get { return Vm.SelectedItemsChanged; }
        }

        public IAuthUser User
        {
            get { return Vm.User; }
        }

        private ObservableAsPropertyHelper<IReadOnlyReactiveList<IPlantViewModel>> _Plants;
        public IReadOnlyReactiveList<IPlantViewModel> Plants
        {
            get
            {
                return _Plants.Value;
            }
        }


        public string Username
        {
            get { return Vm.Username; }
        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
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


        private IReadOnlyReactiveList<IButtonViewModel> _AppBarButtons;
        public new IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _AppBarButtons, value);
            }
        }



    }






}