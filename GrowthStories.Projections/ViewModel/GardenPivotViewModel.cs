
using System;
using System.Reactive.Linq;
using Growthstories.Core;
using ReactiveUI;

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

            // when current plant wants to show the action list, show it
            // moved actionlist to a regular page
            //this.WhenAnyValue(x => x.SelectedPlant)
            //    .Where(x => x != null)
            //    .Select(x => x.ShowActionList as IObservable<object>)
            //    .Switch()
            //    .Subscribe(_ =>
            //    {
            //        this.PlantActionList.Plant = this.SelectedPlant;
            //        // switch back to default when any of the plantactions are clicked
            //        //this.SelectedPlant
            //        //    .NavigateToEmptyActionCommand
            //        //    .Take(1)
            //        //    .Subscribe(x => this.InnerViewModel = null);
            //        //this.NavigateInterface = typeof(IPlantActionListViewModel);

            //        this.SelectedPage = this.PlantActionList; // this makes the buttons and stuff to reflect the settings of the list
            //        this.InnerViewModel = this.PlantActionList;
            //    });

            // when an action is selected on the action list, show default content
            //this.PlantActionList.NavigateToSelected.Subscribe(_ => this.InnerViewModel = null);

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

            //App.BackKeyPressedCommand.OfType<CancelEventArgs>().Subscribe(x =>
            //{
            //    if (this.InnerViewModel == this.PlantActionList)
            //    {
            //        x.Cancel = true;
            //        this.InnerViewModel = null;
            //        this.SelectedPage = this.SelectedPlant;
            //    }
            //});


        }

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

        //public SupportedPageOrientation SupportedOrientations
        //{
        //    get { return SupportedPageOrientation.PortraitOrLandscape; }
        //}

        //protected override SupportedPageOrientation DefaultSupportedOrientation
        //{
        //    get
        //    {
        //        return SupportedPageOrientation.Portrait;
        //    }
        //}


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }






}