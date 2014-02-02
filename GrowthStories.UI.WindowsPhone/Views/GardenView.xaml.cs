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


        protected override void OnViewModelChanged(IGardenViewModel vm)
        {

            var gvm = vm as GardenViewModel;
            if (gvm != null)
            {
                gvm.WhenAnyValue(x => x.OwnGarden).Subscribe(own =>
                {
                    if (own)
                    {
                        MainScroller.Height = 480;
                    }
                    else
                    {
                        MainScroller.Height = 480 + 180;
                    }
                });
            }
        }


        public void handleDelete(PlantViewModel pvm)
        {
            //PlantView.DeleteTile(pvm);
            //MessageBoxResult res = MessageBox.Show("Are you sure you wish to delete the plant " + pvm.Name + "?");
        }


        private void PlantsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.SelectedItemsChanged.Execute(Tuple.Create(e.AddedItems, e.RemovedItems));
        }


        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            Logger.Info("ImageOpened " + sender.ToString());
            ImgOpened(sender, false);
        }


        private void Img_PlaceHolderImageOpened(object sender, RoutedEventArgs e)
        {
            var pvm = GetViewModel(sender);

            if (pvm.Loaded)
            {
                ImgOpened(sender, true);

            }
            else
            {


            }

            var vm = (PlantViewModel)ViewModel.Plants.First();
            var b1 = vm.ShowPlaceHolder;
            var b2 = vm.Loaded;
            var photo = vm.Photo;

            ImgOpened(sender, true);
        }


        private void Img_ImageFailed(object sender, RoutedEventArgs e)
        {
            Logger.Debug("ImageDebug");
            Logger.Info("ImageFailedDebug " + sender.ToString());
        }


        private PlantViewModel GetViewModel(object sender)
        {
            var img = sender as System.Windows.Controls.Image;
            var c4fTile = GSViewUtils.FindParent<Button>(img);
            var button = GSViewUtils.FindParent<Button>(c4fTile);

            return button.CommandParameter as PlantViewModel;
        }


        private void ImgOpened(object sender, bool isPlaceholder)
        {

            var img = sender as System.Windows.Controls.Image;

            DoubleAnimation wa = new DoubleAnimation();
            wa.Duration = new Duration(TimeSpan.FromSeconds(1.5));
            wa.BeginTime = TimeSpan.FromSeconds(0.5);
            wa.From = 0;
            wa.To = 1.0;
            wa.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            Storyboard sb = new Storyboard();
            sb.Children.Add(wa);

            var sp = GSViewUtils.FindParent<StackPanel>(img);

            if (sp != null)
            {
                Storyboard.SetTarget(wa, sp);
                Storyboard.SetTargetProperty(wa, new PropertyPath("Opacity"));
            }

            var c4fTile = GSViewUtils.FindParent<Button>(img);
            var button = GSViewUtils.FindParent<Button>(c4fTile);
            var vm = button.CommandParameter as PlantViewModel;

            if (vm.Photo != null && isPlaceholder)
            {
                return;
            }

            if (vm.Photo == null && !isPlaceholder)
            {
                return;
            }

            // this event is somehow triggered many times, 
            // so do this only once
            // ( double comparison against zero should be ok )
            if (sp.Opacity == 0)
            {
                sb.Begin();
            }
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


    }
}