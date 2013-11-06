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
    public partial class PlantView : UserControl, IViewFor<PlantViewModel>
    {


        public PlantView()
        {
            InitializeComponent();
            SetBinding(ViewModelProperty, new System.Windows.Data.Binding());
        }

        public PlantViewModel ViewModel
        {
            get { return (PlantViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(PlantView), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (PlantViewModel)value; } }

        private void PlantActionView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var plantActionView = (PlantActionView)sender;
            //var plant = ViewModel.SelectedItem;
            ViewModel.ActionTapped.Execute(plantActionView.ViewModel);
        }

    }
}