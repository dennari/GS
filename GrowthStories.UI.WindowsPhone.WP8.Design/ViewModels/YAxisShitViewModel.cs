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
using System.Text.RegularExpressions;

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
            int c = 0;
            var yStep = 0.5;
            //var validTypes = new[] { MeasurementType.LENGTH, MeasurementType.PH, MeasurementType.ILLUMINANCE };
            foreach (var xx in MeasurementTypeHelper.Options.Values)
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
                TelerikSeries[xx.Type] = CreateLineSeries(s, xx);
                if (c > 3)
                    break;
            }

            this._AppBarButtons = new MockReactiveList<IButtonViewModel>();
            c = 0;
            foreach (var m in MeasurementTypeHelper.Options)
            {
                _AppBarButtons.Add(new ButtonViewModel()
                {
                    IconType = m.Value.Icon,
                    Command = ToggleSeries,
                    CommandParameter = m.Key
                });
                c++;
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


        private LineSeries CreateLineSeries(ISeries s, MeasurementTypeHelper xx)
        {
            var l = new LineSeries()
            {
                Stroke = new SolidColorBrush(xx.SeriesColor.ToColor()),
                StrokeThickness = 4,
                Tag = xx.Type
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


        private MockReactiveList<IButtonViewModel> _AppBarButtons;
        public override IReadOnlyReactiveList<IButtonViewModel> AppBarButtons { get { return _AppBarButtons; } }
        //{
        //    get
        //    {
        //        return new MockReactiveList<IButtonViewModel>()
        //        {

        //            new ButtonViewModel()
        //            {
        //                IconType = IconType.MEASURE,
        //                Command = ToggleSeries,
        //                CommandParameter = MeasurementType.LENGTH
        //            },

        //            new ButtonViewModel()
        //            {
        //                IconType = IconType.PH,
        //                Command = ToggleSeries,
        //                CommandParameter = MeasurementType.PH
        //            },

        //            new ButtonViewModel()
        //            {
        //                IconType = IconType.ILLUMINANCE,
        //                Command = ToggleSeries,
        //                CommandParameter = MeasurementType.ILLUMINANCE
        //            },

        //        };
        //    }
        //}





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



public static class ColorMixins
{
    private static Regex _hexColorMatchRegex = new Regex("^#?(?<a>[a-z0-9][a-z0-9])?(?<r>[a-z0-9][a-z0-9])(?<g>[a-z0-9][a-z0-9])(?<b>[a-z0-9][a-z0-9])$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static Color GetColorFromHex(string hexColorString)
    {
        if (hexColorString == null)
            throw new NullReferenceException("Hex string can't be null.");

        // Regex match the string
        var match = _hexColorMatchRegex.Match(hexColorString);

        if (!match.Success)
            throw new InvalidCastException(string.Format("Can't convert string \"{0}\" to argb or rgb color. Needs to be 6 (rgb) or 8 (argb) hex characters long. It can optionally start with a #.", hexColorString));

        // a value is optional
        byte a = 255, r = 0, b = 0, g = 0;
        if (match.Groups["a"].Success)
            a = System.Convert.ToByte(match.Groups["a"].Value, 16);
        // r,b,g values are not optional
        r = System.Convert.ToByte(match.Groups["r"].Value, 16);
        b = System.Convert.ToByte(match.Groups["b"].Value, 16);
        g = System.Convert.ToByte(match.Groups["g"].Value, 16);
        return Color.FromArgb(a, r, b, g);
    }

    public static Color ToColor(this string This)
    {
        return GetColorFromHex(This);
    }
}


//"SOIL_HUMIDITY"=
//{
//  timelineTitle= "soil humidity",
//  seriesTitle= "Soil humidity",
//  seriesColor= "#26a8ba",
//  unit= "%",
//  decimals= 2
//},

///*
//"AIR_HUMIDITY"=
//{
//  timelineTitle= "air humidity",
//  seriesTitle= "Air humidity",
//  seriesColor= "#9bb5d3",
//  unit= "%",
//  decimals= 1
//},
//*/

//"PH"=
//{
//  timelineTitle= "pH",
//  seriesTitle= "pH (acidity)",
//  seriesColor= "#ec9150",
//  unit= "",
//  decimals= 1
//},

//"CO2"=
//{
//  timelineTitle= "CO2",
//  seriesTitle= "CO2",
//  seriesColor= "#ec9150",
//  unit= "ppm",
//  decimals= 0
//}