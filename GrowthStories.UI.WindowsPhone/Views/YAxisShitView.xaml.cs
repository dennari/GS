using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Growthstories.UI.ViewModel;
using Growthstories.UI.WindowsPhone;
using ReactiveUI;
using Telerik.Windows.Controls;
using Telerik.Charting;
using System.Windows.Data;
using System.ComponentModel;
using System.Reactive.Linq;
using Growthstories.Sync;

namespace Growthstories.UI.WindowsPhone
{

    public class YAxisShitViewBase : GSView<IYAxisShitViewModel>
    {

    }

    public partial class YAxisShitView : YAxisShitViewBase
    {
        public YAxisShitView()
        {
            InitializeComponent();
        }

        protected override void OnViewModelChanged(IYAxisShitViewModel pvm)
        {

            // pvm.ToggleTelerikSeries.OfType<LineSeries>().Subscribe(x => Toggle(x));

            //foreach (var s in pvm.TelerikSeries)
            //{
            //    this.
            //}

            //this.RenderChart();
            //this.GSChart.Series.Add(pvm.TelerikSeries.Values.First() as LineSeries);

            var xAxis = this.GSChart.HorizontalAxis as DateTimeContinuousAxis;
            //var yAxis = this.GSChart.VerticalAxis as LinearAxis;



        }

        //private void Toggle(LineSeries x)
        //{

        //    if (GSChart.Series.Contains(x))
        //        GSChart.Series.Remove(x);
        //    else
        //        GSChart.Series.Add(x);


        //}


        //private void RenderChart(IEnumerable<object> dSeries)
        //{
        //    var chart = this.GSChart;

        //    //var lSeries = (LineSeries)chart.Series[0];
        //    //lSeries.Stroke = new SolidColorBrush(Colors.Magenta);
        //    //this.TestText.Text = "RenderChart";
        //    foreach (var s in dSeries)
        //    {
        //        //var series = new LineSeries()
        //        //{
        //        //    Stroke = new SolidColorBrush(Colors.Orange),
        //        //    StrokeThickness = 4
        //        //};

        //        //for (var i = 0; i < s.XValues.Length; i++)
        //        //    series.DataPoints.Add(new CategoricalDataPoint()
        //        //    {
        //        //        Value = s.YValues[i],
        //        //        Category = s.XValues[i]
        //        //    });

        //        //var yx = chart.VerticalAxis as LinearAxis;
        //        //yx.Min
        //        var line = s as LineSeries;
        //        if (line != null)
        //            chart.Series.Add(line);
        //    }
        //}



    }
}