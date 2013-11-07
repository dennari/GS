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


    public sealed class Series : ISeries
    {

        public Tuple<double, double> XRange { get; set; }
        public Tuple<double, double> YRange { get; set; }
        public double[] XValues { get; set; }
        public double[] YValues { get; set; }
        public Tuple<double, double>[] Values { get; set; }

    }


    public class YAxisShitViewModel : RoutableViewModel, IYAxisShitViewModel
    {
        private IPlantViewModel PlantVM;

        protected IDictionary<MeasurementType, ISeries> _Series = new Dictionary<MeasurementType, ISeries>();
        public IDictionary<MeasurementType, ISeries> Series { get { return _Series; } }

        protected IDictionary<MeasurementType, object> _TelerikSeries = new Dictionary<MeasurementType, object>();
        public IDictionary<MeasurementType, object> TelerikSeries { get { return _TelerikSeries; } }

        protected ReactiveCommand _ToggleSeries = new ReactiveCommand();
        public IReactiveCommand ToggleSeries { get { return _ToggleSeries; } }

        protected ReactiveCommand _ToggleTelerikSeries = new ReactiveCommand();
        public IReactiveCommand ToggleTelerikSeries { get { return _ToggleTelerikSeries; } }

        public YAxisShitViewModel(IPlantViewModel plantVM, IGSAppViewModel app)
            : base(app)
        {
            this.PlantVM = plantVM;


            int num = 200;
            var c = 0;
            var yStep = 0.5;
            var validTypes = new[] { MeasurementType.LENGTH, MeasurementType.PH, MeasurementType.ILLUMINANCE };
            foreach (var xx in MeasurementTypeViewModel.GetAll(app).Where(x => validTypes.Contains(x.Type)))
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
                //TelerikSeries[xx.Type] = CreateLineSeries(s, xx.Type);
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



        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }

        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return new ReactiveList<IButtonViewModel>()
                {

                    new ButtonViewModel(App)
                    {
                        IconUri = App.IconUri[IconType.MEASURE],
                        Command = ToggleSeries,
                        CommandParameter = MeasurementType.LENGTH
                    },
                    
                    new ButtonViewModel(App)
                    {
                        IconUri = App.IconUri[IconType.PH],
                        Command = ToggleSeries,
                        CommandParameter = MeasurementType.PH
                    },
                    
                    new ButtonViewModel(App)
                    {
                        IconUri = App.IconUri[IconType.ILLUMINANCE],
                        Command = ToggleSeries,
                        CommandParameter = MeasurementType.ILLUMINANCE
                    },

                };
            }
        }

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
    }
}
