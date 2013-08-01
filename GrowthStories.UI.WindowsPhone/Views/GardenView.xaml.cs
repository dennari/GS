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

namespace GrowthStories.UI.WindowsPhone.Views
{
    public partial class GardenView : PhoneApplicationPage
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

    }
}