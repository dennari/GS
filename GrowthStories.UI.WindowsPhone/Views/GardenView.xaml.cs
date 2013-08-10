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
using Growthstories.Domain.Messaging;
using ReactiveUI;

namespace GrowthStories.UI.WindowsPhone.Views
{
    public partial class GardenView : PhoneApplicationPage, IViewFor<GardenViewModel>
    {
        public GardenView()
        {
            InitializeComponent();
        }

        private GardenViewModel VM
        {
            get
            {
                return (GardenViewModel)this.DataContext;
            }
        }

        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    this.VM.LoadPlants();
        //}


        public GardenViewModel ViewModel
        {
            get { return (GardenViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(GardenViewModel), typeof(GardenView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (GardenViewModel)value; }
        }
    }
}