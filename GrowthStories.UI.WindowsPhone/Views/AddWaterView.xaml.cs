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
using Growthstories.UI.WindowsPhone.ViewModels;

namespace Growthstories.UI.WindowsPhone
{
    public partial class AddWaterView : UserControl, IViewFor<PlantWaterViewModel>
    {
        public AddWaterView()
        {
            InitializeComponent();

        }

        public PlantWaterViewModel ViewModel
        {
            get { return (PlantWaterViewModel)GetValue(ViewModelProperty); }
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
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(AddWaterView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (PlantWaterViewModel)value; } }

    }
}