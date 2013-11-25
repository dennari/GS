using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;

namespace Growthstories.UI.ViewModel
{



    public class YAxisShitViewModel : RoutableViewModel, IYAxisShitViewModel
    {
        private IPlantViewModel PlantVM;

        public YAxisShitViewModel(IPlantViewModel plantVM, IGSAppViewModel app)
            : base(app)
        {
            this.PlantVM = plantVM;
            this.XAxisLabelStep = 1;
            this.Minimum = double.MinValue;
            this.Maximum = double.MaxValue;

            this.AppBarButtons = this.EnabledSeries.CreateDerivedCollection(x =>
            {

                return new ButtonViewModel()
                {
                    IconType = MeasurementTypeHelper.Options[x.Item1].Icon,
                    Command = ToggleSeries,
                    CommandParameter = x.Item1
                };

            });


            var allActions = PlantVM.Actions;
            //var allActions = CreateFakeData();

            foreach (var x in MeasurementTypeHelper.Options)
            {
                var series = allActions.CreateDerivedCollection(y => (IPlantMeasureViewModel)y, y => y is IPlantMeasureViewModel && y.MeasurementType == x.Key);
                AllSeries[x.Key] = series;

                //series.ItemsAdded.Subscribe(x => {
                //    if(series.C)
                //})

                series.ItemsAdded.Select(z => series.Count).StartWith(series.Count).Subscribe(y =>
                {
                    if (y >= 2)
                    {
                        var tuple = new Tuple<MeasurementType, IReadOnlyReactiveList<IPlantMeasureViewModel>>(x.Key, series);
                        this.EnabledSeries.Add(tuple);
                        if (Series == null)
                            SetSeries(x.Key, series);
                    }
                });
                series.ItemsRemoved.Select(_ => series.Count).StartWith(series.Count).Subscribe(y =>
                {
                    if (y < 2)
                    {
                        var match = this.EnabledSeries.FirstOrDefault(z => z.Item1 == x.Key);
                        if (match != null)
                            this.EnabledSeries.Remove(match);
                        if (Series == series)
                            Series = null;
                    }
                });




            }

            this.ToggleSeries
                .OfType<MeasurementType>()
                .Subscribe(z =>
                {
                    var match = EnabledSeries.FirstOrDefault(y => y.Item1 == z);
                    if (match != null)
                        SetSeries(z, match.Item2);
                });

        }

        protected virtual void SetSeries(MeasurementType type, IReadOnlyReactiveList<IPlantMeasureViewModel> series)
        {
            var m = MeasurementTypeHelper.Options[type];
            this.Series = series;
            this.LineColor = m.SeriesColor;
            this.SeriesTitle = m.TitleWithUnit;

            this.XAxisLabelStep = (int)Math.Ceiling((double)series.Count / NumLabels);
            //var range = Series.Last().Created - Series.First().Created;

        }

        protected Dictionary<MeasurementType, IReadOnlyReactiveList<IPlantMeasureViewModel>> AllSeries = new Dictionary<MeasurementType, IReadOnlyReactiveList<IPlantMeasureViewModel>>();
        protected ReactiveList<Tuple<MeasurementType, IReadOnlyReactiveList<IPlantMeasureViewModel>>> EnabledSeries = new ReactiveList<Tuple<MeasurementType, IReadOnlyReactiveList<IPlantMeasureViewModel>>>();

        public const int NumLabels = 12;

        private double _Minimum;
        public double Minimum
        {
            get
            {
                return _Minimum;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Minimum, value);
            }
        }

        private double _Maximum;
        public double Maximum
        {
            get
            {
                return _Maximum;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Maximum, value);
            }
        }

        private double _YAxisStep;
        public double YAxisStep
        {
            get
            {
                return _YAxisStep;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _YAxisStep, value);
            }
        }

        private int _YAxisLabelStep;
        public int YAxisLabelStep
        {
            get
            {
                return _YAxisLabelStep;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _YAxisLabelStep, value);
            }
        }

        private int _XAxisLabelStep;
        public int XAxisLabelStep
        {
            get
            {
                return _XAxisLabelStep;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _XAxisLabelStep, value);
            }
        }

        private string _LineColor;
        public string LineColor
        {
            get
            {
                return _LineColor;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _LineColor, value);
            }
        }



        private IReadOnlyReactiveList<IPlantMeasureViewModel> _Series;
        public IReadOnlyReactiveList<IPlantMeasureViewModel> Series
        {
            get
            {
                return _Series;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Series, value);
            }
        }

        private string _SeriesTitle;
        public string SeriesTitle
        {
            get
            {
                return _SeriesTitle;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SeriesTitle, value);
            }
        }



        protected ReactiveCommand _ToggleSeries = new ReactiveCommand();
        public IReactiveCommand ToggleSeries { get { return _ToggleSeries; } }


        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }

        //protected ReactiveList<IButtonViewModel> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons { get; protected set; }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        private IReadOnlyReactiveList<IPlantActionViewModel> CreateFakeData()
        {
            int num = 10;
            var series = new IPlantActionViewModel[num * MeasurementTypeHelper.Options.Count];

            var XRange = Tuple.Create((double)0, 6 * Math.PI);
            var YRange = Tuple.Create((double)0, (double)1);
            int c = 0;
            //var yStep = 0.5;
            //var validTypes = new[] { MeasurementType.LENGTH, MeasurementType.PH, MeasurementType.ILLUMINANCE };
            foreach (var xx in MeasurementTypeHelper.Options.Values)
            {

                double step = (XRange.Item2 - XRange.Item1) / num;
                double x = XRange.Item1;
                var beginning = DateTimeOffset.Now - TimeSpan.FromDays(num * 5);
                //s.XValues[0] = x;
                for (var i = 0; i < num; i++)
                {
                    //s.YValues[i] = Math.Sin(x);// + c * yStep;
                    //s.XValues[i] = x;
                    //s.Values[i] = Tuple.Create(s.XValues[i], s.YValues[i]);
                    series[num * c + i] = new PlantMeasureViewModel(App, new PlantActionState(new PlantActionCreated(new CreatePlantAction(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), PlantActionType.MEASURED, "dfgd")
                    {
                        Value = Math.Sin(x + c),
                        MeasurementType = xx.Type
                    })
                    {
                        Created = beginning + TimeSpan.FromDays(3 * i)
                    }
                    ));


                    x += step;
                }
                c++;

            }

            return new ReactiveList<IPlantActionViewModel>(series);
        }

    }
}
