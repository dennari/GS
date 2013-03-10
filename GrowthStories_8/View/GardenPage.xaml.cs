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
using Growthstories.PCL.Models;
using Growthstories.PCL.Services;

namespace Growthstories.WP8.View
{
    public partial class GardenPage : PhoneApplicationPage
    {
        public GardenPage()
        {

            InitializeComponent();
            var vm = this.DataContext as GardenViewModel;
            vm.MyGarden.Plants.Add(new Plant(new FakePlantDataService())
            {
                Genus = "Phytoliforus",
                Name = "Kipa",
                ProfilePicturePath = "/Assets/ApplicationIcon.png"
            });
        }


        private void newPlantAppBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/AddPlant.xaml", UriKind.Relative));
        }

        //override protected void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    var vm = this.DataContext as GardenViewModel;
        //    this.DataContext = null;
        //    this.DataContext = vm;
        //}



    }
}