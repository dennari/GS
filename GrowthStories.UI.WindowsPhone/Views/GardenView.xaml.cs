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
using System.Windows.Data;

namespace Growthstories.UI.WindowsPhone
{
    public class GardenViewBase : GSView<IGardenViewModel>
    {

    }

    public partial class GardenView : GardenViewBase
    {



        public GardenView()
        {
            InitializeComponent();
        }


        private void PlantsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.SelectedItemsChanged.Execute(Tuple.Create(e.AddedItems, e.RemovedItems));
        }


    }
}