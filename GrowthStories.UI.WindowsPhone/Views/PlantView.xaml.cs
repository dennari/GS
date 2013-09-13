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
using GrowthStories.UI.WindowsPhone;
using ReactiveUI;

namespace GrowthStories.UI.WindowsPhone
{
    public partial class PlantView : UserControl, IViewFor<PlantViewModel>
    {
        public PlantView()
        {
            InitializeComponent();

        }

        public PlantViewModel ViewModel
        {
            get { return (PlantViewModel)GetValue(ViewModelProperty); }
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
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(PlantView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (PlantViewModel)value; } }

    }
}