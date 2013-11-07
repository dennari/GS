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
using System.Linq;
using Growthstories.Sync;

namespace Growthstories.UI.WindowsPhone
{


    public partial class YAxisShitView : UserControl, IViewFor<IYAxisShitViewModel>, IReactsToViewModelChange
    {
        public YAxisShitView()
        {
            InitializeComponent();
            this.SetBinding(ViewModelProperty, new Binding());
            //var lSeries = (LineSeries)GSChart.Series[0];
            //lSeries.Stroke = new SolidColorBrush(Colors.Magenta);
            //this.TestText.Text = "Constructor";

            if (DesignerProperties.IsInDesignTool)
            {
                //var vm = new YAxisShitViewModel();
                //this.ViewModel = vm;
                //this.RenderChart(vm.TelerikSeries.Values);
            }
        }

        public IYAxisShitViewModel ViewModel
        {
            get { return (IYAxisShitViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(YAxisShitView), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (IYAxisShitViewModel)value; } }

        public void ViewModelChanged(object vm)
        {
            var pvm = vm as IYAxisShitViewModel;
            if (pvm != null)
            {
                pvm.ToggleTelerikSeries.OfType<LineSeries>().Subscribe(x => Toggle(x));
                //RenderChart(pvm.TelerikSeries.Values);
            }
            //this.RenderChart(pvm.TelerikSeries);
        }

        private void Toggle(LineSeries x)
        {

            if (GSChart.Series.Contains(x))
                GSChart.Series.Remove(x);
            else
                GSChart.Series.Add(x);


        }


        private void RenderChart(IEnumerable<object> dSeries)
        {
            var chart = this.GSChart;

            //var lSeries = (LineSeries)chart.Series[0];
            //lSeries.Stroke = new SolidColorBrush(Colors.Magenta);
            //this.TestText.Text = "RenderChart";
            foreach (var s in dSeries)
            {
                //var series = new LineSeries()
                //{
                //    Stroke = new SolidColorBrush(Colors.Orange),
                //    StrokeThickness = 4
                //};

                //for (var i = 0; i < s.XValues.Length; i++)
                //    series.DataPoints.Add(new CategoricalDataPoint()
                //    {
                //        Value = s.YValues[i],
                //        Category = s.XValues[i]
                //    });

                //var yx = chart.VerticalAxis as LinearAxis;
                //yx.Min
                var line = s as LineSeries;
                if (line != null)
                    chart.Series.Add(line);
            }
        }



    }
}