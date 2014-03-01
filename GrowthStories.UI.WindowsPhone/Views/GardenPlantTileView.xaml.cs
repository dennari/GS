﻿using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using Growthstories.UI.ViewModel;
using ReactiveUI;


namespace Growthstories.UI.WindowsPhone
{


    public class GardenPlantTileViewBase : GSView<IPlantViewModel>
    {

    }


    public partial class GardenPlantTileView : GardenPlantTileViewBase
    {


        public GardenPlantTileView()
        {
            InitializeComponent();

            LoadingPhoto.Visibility = Visibility.Visible;

            //if (OwnGarden != null || OwnGarden.Equals("FALSE"))
            //{
            //}

        }

        IDisposable subs = Disposable.Empty;


        //public static readonly DependencyProperty OwnGardenProperty =
        //     DependencyProperty.Register("OwnGarden", typeof(string), typeof(GardenView), new PropertyMetadata("TRUE"));

        //public string OwnGarden
        //{
        //    get
        //    {
        //        return (string)GetValue(OwnGardenProperty);
        //    }
        //    set
        //    {
        //        SetValue(OwnGardenProperty, value);
        //    }
        //}


        protected override void OnViewModelChanged(IPlantViewModel vm)
        {
            //ViewModel.Log().Info("GardenPlantTileView: onviewmodelchanged gardenplanttileview " + vm.Name);
            //ViewModel.Log().Info("GardenPlantTileView: plant loaded is " + vm.Loaded);
            //ViewModel.Log().Info("GardenPlantTileView: vw has writeaccess is " + vm.HasWriteAccess);
            if (vm == null)
                return;

            subs.Dispose();
            subs = Observable.CombineLatest(
                vm.WhenAnyValue(x => x.ShowPlaceHolder).Where(x => x),
                vm.WhenAnyValue(z => z.Loaded).Where(z => z))

                .Take(1).Subscribe(_ =>
            {
                if (vm.ShowPlaceHolder)
                {
                    _FadeIn();
                }
                else if (vm.Loaded)
                {
                    if (Opened)
                    {
                        ViewModel.Log().Info("GardenPlantTileView: plant loading ready, fading in plant " + ViewModel.Name);
                        _FadeIn();
                    }
                }
            });
        }


        private bool Opened = false;

        public static HashSet<Guid> OpenedImages = new HashSet<Guid>();


        // Real image (no placeholder) has been opened
        // 
        //
        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            OpenedImages.Add(this.ViewModel.Id);
            ViewModel.Log().Info("GardenPlantTileView: image opened event for " + ViewModel.Name);
            Opened = true;
            if (ViewModel.Loaded)
            {
                FadeIn();
            }
        }

        private void FadeIn()
        {
            _FadeIn();
            subs.Dispose();
        }

        // Fade the content in if not already faded/fading in
        //
        //
        private void _FadeIn()
        {
            LoadingFailed.Visibility = Visibility.Collapsed;
            LoadingPhoto.Opacity = 0.0;

            DoubleAnimation wa = new DoubleAnimation();
            wa.Duration = new Duration(TimeSpan.FromSeconds(1.2));
            wa.BeginTime = TimeSpan.FromSeconds(0.2);
            wa.From = 0;
            wa.To = 1.0;
            wa.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            Storyboard sb = new Storyboard();
            sb.Children.Add(wa);

            Storyboard.SetTarget(wa, panel);
            Storyboard.SetTargetProperty(wa, new PropertyPath("Opacity"));

            sb.Completed += FadeInCompleted;

            if (panel.Opacity == 0)
            {
                ViewModel.Log().Info("GardenPlantTileView: starting fadein for " + ViewModel.Name);
                sb.Begin();
            }
            else
            {
                ViewModel.Log().Info("GardenPlantTileView: skipping fadein for " + ViewModel.Name);
            }


        }


        private void FadeInCompleted(object sender, EventArgs e)
        {
            trexStoryboard.Begin();
        }


        private void Img_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("GardenPlantTileView: image loaded for " + ViewModel.Name);
            if (OpenedImages.Contains(this.ViewModel.Id))
            {
                FadeIn();
            }
        }


        private void Img_ImageFailed(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("GardenPlantTileView: image failed to load for " + ViewModel.Name);
            ViewModel.NotifyImageDownloadFailed();
            FadeIn();
            LoadingFailed.Visibility = Visibility.Visible;
        }


        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ShowDetailsCommand.Execute(ViewModel);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RadContextMenu_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        ~GardenPlantTileView()
        {
            NotifyDestroyed("");
        }


        private void LayoutRoot_Unloaded(object sender, RoutedEventArgs e)
        {
            trexStoryboard.Stop();
        }


        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (Opened)
            {
                trexStoryboard.Begin();
            }
        }
 

    }


}