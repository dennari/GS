using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Growthstories.UI.ViewModel;
using Growthstories.UI.WindowsPhone;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{
    public partial class AddMeasurementView : UserControl, IViewFor<AddMeasurementViewModel>
    {
        public AddMeasurementView()
        {
            InitializeComponent();

        }

        public AddMeasurementViewModel ViewModel
        {
            get { return (AddMeasurementViewModel)GetValue(ViewModelProperty); }
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
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(AddMeasurementView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (AddMeasurementViewModel)value; } }

    }
}