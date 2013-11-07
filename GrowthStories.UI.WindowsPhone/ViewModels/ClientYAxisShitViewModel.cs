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
using Growthstories.UI.ViewModel;

namespace Growthstories.UI.WindowsPhone.ViewModels
{


    public class ClientYAxisShitViewModel : YAxisShitViewModel
    {


        //protected IDictionary<MeasurementType, LineSeries> _TelerikSeries = new Dictionary<MeasurementType, LineSeries>();
        //public IDictionary<MeasurementType, LineSeries> TelerikSeries { get { return _TelerikSeries; } }


        //protected ReactiveCommand _ToggleTelerikSeries = new ReactiveCommand();
        //public IReactiveCommand ToggleTelerikSeries { get { return _ToggleTelerikSeries; } }



        public ClientYAxisShitViewModel(IPlantViewModel plantVM, IGSAppViewModel app)
            : base(plantVM, app)
        {




            ToggleSeries.OfType<MeasurementType>().ObserveOn(RxApp.MainThreadScheduler).Subscribe(m =>
            {
                //var current = TelerikSeries.FirstOrDefault(y => (MeasurementType)y.Tag == (MeasurementType)m);
                //if (current != null)
                lock (_TelerikSeries)
                {
                    if (Series.ContainsKey(m) && !TelerikSeries.ContainsKey(m))
                        TelerikSeries[m] = CreateLineSeries(Series[m], m);
                }
                ToggleTelerikSeries.Execute(TelerikSeries[m]);


            });


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


    }




}
