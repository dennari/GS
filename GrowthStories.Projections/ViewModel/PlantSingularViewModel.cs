
using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{


    public class PlantSingularViewModel : MultipageViewModel, IPlantSingularViewModel
    {

        public IPlantViewModel Plant { get; private set; }
        // just to keep compiler happy
        public IReactiveCommand ShowDetailsCommand { get; private set; }

        private IRoutableViewModel _InnerViewModel;
        public IRoutableViewModel InnerViewModel
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

        public PlantSingularViewModel(IPlantViewModel plant, IGSAppViewModel app)
            : base(app)
        {
            this.Plant = plant;



            //plant.ShowActionList.Subscribe(_ =>
            //   {


            //       this.SelectedPage = this.PlantActionList; // this makes the buttons and stuff to reflect the settings of the list
            //       this.InnerViewModel = this.PlantActionList;
            //   });

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
                    if ((o & PageOrientation.Landscape) == PageOrientation.Landscape)
                    {
                        this.SelectedPage = this.Plant.Chart;
                        this.InnerViewModel = this.Plant.Chart;
                    }
                    else
                    {
                        this.InnerViewModel = null;
                        this.SelectedPage = this.Plant;
                    }
                    //App.Router.Navigate.Execute(this.CurrentChartViewModel);
                });

            //App.BackKeyPressedCommand.OfType<CancelEventArgs>().Subscribe(x =>
            //{
            //    if (this.InnerViewModel == this.PlantActionList)
            //    {
            //        x.Cancel = true;
            //        this.InnerViewModel = null;
            //        this.SelectedPage = this.Plant;
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
                    _PlantActionList = new PlantActionListViewModel(Plant, App);
                }
                return _PlantActionList;
            }
        }

        public Guid Id
        {
            get { return Plant.Id; }
        }

        public Guid UserId
        {
            get { return Plant.UserId; }
        }



        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }






}