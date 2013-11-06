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
    public partial class GardenPivotView : UserControl, IViewFor<GardenViewModel>
    {


        public GardenPivotView()
        {
            InitializeComponent();

        }

        public GardenViewModel ViewModel
        {
            get { return (GardenViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                    this.DataContext = value;
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(GardenPivotView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (GardenViewModel)value; } }

        private void PlantActionView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var plantActionView = (PlantActionView)sender;
            var plant = ViewModel.SelectedItem;
            plant.ActionTapped.Execute(plantActionView.ViewModel);
        }



    }
}