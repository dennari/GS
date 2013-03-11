using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Growthstories.PCL.ViewModel;

namespace Growthstories.WP8.View
{
    public partial class PlantPage : PhoneApplicationPage
    {
        public PlantPage()
        {
            InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //var pvm = DataContext as PlantViewModel;
        }
    }
}