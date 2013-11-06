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

namespace Growthstories.UI.WindowsPhone
{
    public partial class YAxisShitView : UserControl, IViewFor<IPlantViewModel>
    {
        public YAxisShitView()
        {
            InitializeComponent();
            this.ViewModel = new PlantViewModel();
        }

        public IPlantViewModel ViewModel
        {
            get { return (IPlantViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(YAxisShitView), new PropertyMetadata(null, ViewModelValueChanged));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (IPlantViewModel)value; } }


        static void ViewModelValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var view = (YAxisShitView)sender;
                view.SetDataContext((IPlantViewModel)e.NewValue);

            }
            catch { }
        }

        private void SetDataContext(IPlantViewModel vm)
        {
            this.DataContext = vm;
            this.RenderChart(vm.Series);
        }

        private void RenderChart(IEnumerable<ISeries> dSeries)
        {
            var chart = this.GSChart;

            foreach (var s in dSeries)
            {
                var series = new LineSeries()
                {
                    Stroke = new SolidColorBrush(Colors.Orange),
                    StrokeThickness = 2
                };

                for (var i = 0; i < s.XValues.Length; i++)
                    series.DataPoints.Add(new CategoricalDataPoint()
                    {
                        Value = s.YValues[i],
                        Category = s.XValues[i]
                    });

                var yx = chart.VerticalAxis as LinearAxis;
                //yx.Min
                chart.Series.Add(series);
            }
        }

    }
}