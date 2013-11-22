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


    public class GardenPivotViewBase : GSPage<IGardenPivotViewModel>
    {

    }
    public partial class GardenPivotView : GardenPivotViewBase
    {


        public GardenPivotView()
        {
            InitializeComponent();

        }


        //private void PlantActionView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    var plantActionView = (PlantActionView)sender;
        //    var plant = ViewModel.SelectedItem;
        //    plant.ActionTapped.Execute(plantActionView.ViewModel);
        //}



    }
}