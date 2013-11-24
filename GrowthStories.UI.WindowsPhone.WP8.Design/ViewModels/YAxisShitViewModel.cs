using Growthstories.Domain.Entities;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Windows.Media.Imaging;
using Growthstories.Domain.Messaging;
//using Telerik.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace Growthstories.UI.ViewModel
{



    public sealed class Series : ISeries
    {

        public Tuple<double, double> XRange { get; set; }
        public Tuple<double, double> YRange { get; set; }
        public double[] XValues { get; set; }
        public double[] YValues { get; set; }
        public Tuple<double, double>[] Values { get; set; }

    }


    public class YAxisShitViewModel : DesignViewModelBase, IYAxisShitViewModel
    {
        private PlantViewModel PlantVM;

        protected IDictionary<MeasurementType, ISeries> _Series = new Dictionary<MeasurementType, ISeries>();
        public IDictionary<MeasurementType, ISeries> Series { get { return _Series; } }

        protected IDictionary<MeasurementType, object> _TelerikSeries = new Dictionary<MeasurementType, object>();
        public IDictionary<MeasurementType, object> TelerikSeries { get { return _TelerikSeries; } }


        protected MockReactiveCommand _ToggleSeries = new MockReactiveCommand();
        public IReactiveCommand ToggleSeries { get { return _ToggleSeries; } }

        protected MockReactiveCommand _ToggleTelerikSeries = new MockReactiveCommand();
        public IReactiveCommand ToggleTelerikSeries { get { return _ToggleTelerikSeries; } }

        public YAxisShitViewModel()
            : this(new PlantViewModel())
        {

        }

        public YAxisShitViewModel(PlantViewModel plantVM)
        {
            this.PlantVM = plantVM;


            int num = 200;
            var c = 0;
            var yStep = 0.5;
            var validTypes = new[] { MeasurementType.LENGTH, MeasurementType.PH, MeasurementType.ILLUMINANCE };
            foreach (var xx in MeasurementTypeHelper.Options.Values.Where(x => validTypes.Contains(x.Type)))
            {


                var s = new Series()
                {
                    YValues = new double[num],
                    XValues = new double[num],
                    Values = new Tuple<double, double>[num],
                    XRange = Tuple.Create((double)0, 6 * Math.PI),
                    YRange = Tuple.Create((double)0, (double)1)
                };

                double step = (s.XRange.Item2 - s.XRange.Item1) / num;
                double x = s.XRange.Item1;
                //s.XValues[0] = x;
                for (var i = 0; i < num; i++)
                {
                    s.YValues[i] = Math.Sin(x) + c * yStep;
                    s.XValues[i] = x;
                    s.Values[i] = Tuple.Create(s.XValues[i], s.YValues[i]);
                    x += step;
                }
                c++;

                //_TelerikSeries.Add(CreateLineSeries(s, xx, c));
                Series[xx.Type] = s;
                TelerikSeries[xx.Type] = CreateLineSeries(s, xx.Type);
                if (c > 3)
                    break;
            }

            //ToggleSeries.OfType<MeasurementType>().ObserveOn(RxApp.MainThreadScheduler).Subscribe(m =>
            //{
            //    //var current = TelerikSeries.FirstOrDefault(y => (MeasurementType)y.Tag == (MeasurementType)m);
            //    //if (current != null)
            //    lock (_TelerikSeries)
            //    {
            //        if (Series.ContainsKey(m) && !TelerikSeries.ContainsKey(m))
            //            TelerikSeries[m] = CreateLineSeries(Series[m], m);
            //    }
            //    ToggleTelerikSeries.Execute(TelerikSeries[m]);


            //});
            //var a = new RadCartesianChart();
            //var b = new PresenterCollection<LineSeries>();
            //b.Add(new LineSeries);

        }

        public static Dictionary<MeasurementType, Color> LineColors = new Dictionary<MeasurementType, Color>()
        {
            {MeasurementType.LENGTH,Colors.Blue},
            {MeasurementType.ILLUMINANCE,Colors.Brown},
            {MeasurementType.PH,Colors.Cyan}
        };

        private LineSeries CreateLineSeries(ISeries s, MeasurementType xx)
        {
            var l = new LineSeries()
            {
                Stroke = new SolidColorBrush(LineColors[xx]),
                StrokeThickness = 3,
                Tag = xx

            };

            s.Values.Aggregate(0, (acc, v) =>
            {
                l.DataPoints.Add(new Telerik.Charting.CategoricalDataPoint() { Value = v.Item2, Category = v.Item1 });
                return acc + 1;
            });

            return l;

        }

        //ReactiveList<LineSeries> _TelerikSeries = new ReactiveList<LineSeries>();
        //public IReadOnlyReactiveList<LineSeries> TelerikSeries { get { return _TelerikSeries; } }


        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.Landscape; }
        }

        public override IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return new MockReactiveList<IButtonViewModel>()
                {

                    new ButtonViewModel()
                    {
                        IconType = IconType.MEASURE,
                        Command = ToggleSeries,
                        CommandParameter = MeasurementType.LENGTH
                    },
                    
                    new ButtonViewModel()
                    {
                        IconType = IconType.PH,
                        Command = ToggleSeries,
                        CommandParameter = MeasurementType.PH
                    },
                    
                    new ButtonViewModel()
                    {
                        IconType = IconType.ILLUMINANCE,
                        Command = ToggleSeries,
                        CommandParameter = MeasurementType.ILLUMINANCE
                    },

                };
            }
        }

        public override ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public override bool AppBarIsVisible
        {
            get { return true; }
        }
    }




}
