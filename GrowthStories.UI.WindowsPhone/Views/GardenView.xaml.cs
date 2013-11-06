using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using Growthstories.UI.ViewModel;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{
    public partial class GardenView : UserControl, IViewFor<IGardenViewModel>
    {

        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (IGardenViewModel)value; } }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IGardenViewModel), typeof(GardenView), new PropertyMetadata(null, ViewModelValueChanged));

        static void ViewModelValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var view = (GardenView)sender;
                view.DataContext = (IGardenViewModel)e.NewValue;

            }
            catch { }
        }

        public IGardenViewModel ViewModel
        {
            get { return (IGardenViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                }
            }
        }

        public GardenView()
        {
            InitializeComponent();
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }



    }
}