using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System.Reactive.Disposables;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using GrowthStories.UI.WindowsPhone.BA;
using EventStore.Logging;
using System.Windows.Controls.Primitives;


namespace Growthstories.UI.WindowsPhone
{


    public class GardenPlantTileViewBase : GSView<IPlantViewModel>
    {

    }


    public partial class GardenPlantTileView : GardenPlantTileViewBase
    {


        public static bool Complained = false;


        public GardenPlantTileView()
        {
            InitializeComponent();
        }


        private IDisposable subscription;



        protected override void OnViewModelChanged(IPlantViewModel vm)
        {

            if (subscription != null)
            {
                subscription.Dispose();
            }

            subscription = vm.WhenAnyValue(x => x.ShowPlaceHolder).Subscribe(x =>
            {
                if (x)
                {
                    ViewModel.Log().Info("plant loading ready, loading placeholder for " + ViewModel.Name);
                    FadeIn();
                }
                else
                {
                    ViewModel.Log().Info("plant loading ready (does have profile picture) for " + ViewModel.Name);
                }
            });

        }
        


        // Real image (no placeholder) has been opened
        // 
        //
        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("image opened event for " + ViewModel.Name);
            FadeIn();
        }


        // Fade the content in if not already faded/fading in
        //
        //
        private void FadeIn()
        {
            DoubleAnimation wa = new DoubleAnimation();
            wa.Duration = new Duration(TimeSpan.FromSeconds(1.2));
            wa.BeginTime = TimeSpan.FromSeconds(0.1);
            wa.From = 0;
            wa.To = 1.0;
            wa.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            Storyboard sb = new Storyboard();
            sb.Children.Add(wa);

            Storyboard.SetTarget(wa, panel);
            Storyboard.SetTargetProperty(wa, new PropertyPath("Opacity"));

            if (panel.Opacity == 0)
            {
                ViewModel.Log().Info("starting fadein for " + ViewModel.Name);
                sb.Begin();
            }
            else
            {
                ViewModel.Log().Info("skipping fadein for " + ViewModel.Name);
            }
        }


        private void Img_ImageFailed(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("image failed to load for " + ViewModel.Name);

            ViewModel.NotifyImageDownloadFailed();
        }


        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ShowDetailsCommand.Execute(null);
            }
        }

    }


      


}