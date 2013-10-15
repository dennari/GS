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
using BindableApplicationBar;
using Growthstories.UI.ViewModel;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{
    public partial class GardenView : UserControl
    {


        public GardenView()
        {
            InitializeComponent();
            //FriendsSelector.SelectedItem = null;
        }




        private void PlantsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


    }
}