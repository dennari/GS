﻿using System;
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


    public class GardenPivotViewBase : GSView<IGardenPivotViewModel>
    {

    }
    public partial class GardenPivotView : GardenPivotViewBase
    {


        public GardenPivotView()
        {
            InitializeComponent();

        }

        protected override void OnViewModelChanged(IGardenPivotViewModel vm)
        {
            //base.OnViewModelChanged(vm);
            vm.Log().Info("GardenPivotView: OnViewModelChanged");

        }




        private void Plants_Loaded(object sender, RoutedEventArgs e)
        {
            //foreach (var pvm in ViewModel.Plants)
            //{
            //    pvm.ResetAnimationsCommand.Execute(null);
            //}
        }

        //private void PlantActionView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    var plantActionView = (PlantActionView)sender;
        //    var plant = ViewModel.SelectedItem;
        //    plant.ActionTapped.Execute(plantActionView.ViewModel);
        //}


        public void CleanUp()
        {
            //
            //foreach (var p in ViewModel.Plants)
            //{
            //    p.ShouldBeFullyLoaded = false;
            //}
        }


    }
}