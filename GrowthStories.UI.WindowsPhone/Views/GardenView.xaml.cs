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
using EventStore.Logging;
using System.Windows.Media.Animation;
using System.Windows.Media;
using Growthstories.Core;
using Growthstories.UI.WindowsPhone.ViewModels;

namespace Growthstories.UI.WindowsPhone
{

    public class GardenViewBase : GSView<IGardenViewModel>
    {

    }


    public partial class GardenView : GardenViewBase
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(GardenView));


        public GardenView()
        {
            InitializeComponent();

            if (Height != Double.NaN)
            {
                Height = Double.NaN;
            }

            Logger.Info("initialized garden view");
        }


        public static readonly DependencyProperty CleanUpOnUnloadProperty = 
            DependencyProperty.Register("CleanUpOnUnload", typeof(string), typeof(GardenView), new PropertyMetadata("FALSE"));


        public static readonly DependencyProperty MainScrollerHeightProperty =
            DependencyProperty.Register("MainScrollerHeight", typeof(int), typeof(GardenView), new PropertyMetadata(480));


        public string CleanUpOnUnload
        {
            get
            {
                return (string)GetValue(CleanUpOnUnloadProperty);
            }
            set
            {
                SetValue(CleanUpOnUnloadProperty, value);
            }
        }


        public int MainScrollerHeight
        {
            get
            {
                return (int)GetValue(MainScrollerHeightProperty);
            }
            set
            {
                SetValue(MainScrollerHeightProperty, value);
            }
        }


        protected override void OnViewModelChanged(IGardenViewModel vm)
        {
            MainScroller.Height = MainScrollerHeight;

            //var gvm = vm as GardenViewModel;
            //if (gvm != null)
            //{
            //    gvm.WhenAnyValue(x => x.OwnGarden).Subscribe(own =>
            //    {
            //        if (own)
            //        {
            //            MainScroller.Height = 480;
            //        }
            //        else
            //        {
            //            MainScroller.Height = 480 + 180;
            //        }
            //    });
            //}
        }


        private void PlantsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.SelectedItemsChanged.Execute(Tuple.Create(e.AddedItems, e.RemovedItems));
        }


        private PlantViewModel GetViewModel(object sender)
        {
            var img = sender as System.Windows.Controls.Image;
            var c4fTile = GSViewUtils.FindParent<Button>(img);
            var button = GSViewUtils.FindParent<Button>(c4fTile);

            return button.CommandParameter as PlantViewModel;
        }


        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // for some reason command triggering did not work for the button
            // so we are doing it this way
            //   -- JOJ 5.12.2014

            var btn = sender as Button;
            if (ViewModel != null)
            {
                ViewModel.ShowDetailsCommand.Execute(btn.CommandParameter);
            }
        }

        private void ViewRoot_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("gardenview unloaded for {0}, CleanUpOnUnload is {1}", ViewModel.Username, CleanUpOnUnload);
            if (CleanUpOnUnload != null && CleanUpOnUnload.Equals("TRUE"))
            {
                ViewModel.Log().Info("cleaning up gardenview {0}", ViewModel.Username);
                PlantsSelector.IsSelectionEnabled = false;
                PlantsSelector.ItemsSource = null;
                PlantsSelector.SelectionChanged -= PlantsSelector_SelectionChanged;
                
                ViewHelpers.ClearLongListMultiSelectorDependencyValues(PlantsSelector);
            }
        }


        ~GardenView()
        {
            NotifyDestroyed("");
        }

    }
}