using System;
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

        //IDisposable subs = Disposable.Empty;


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


        List<IDisposable> subs = new List<IDisposable>();


        private void DisposeSubs()
        {
            foreach (var s in new List<IDisposable>(subs))
            {
                s.Dispose();
            }
            subs.Clear();
        }


        protected override void OnViewModelChanged(IPlantViewModel vm)
        { 
            if (vm == null)
                return;

            ViewModel.Log().Info("GardenPlantTileView: onviewmodelchanged gardenplanttileview " + vm.Name);
           
            subs.Add(vm.WhenAnyValue(x => x.ShowPlaceHolder).Where(x => x).Subscribe(_ =>
            {
                FadeIn();
            }));

            subs.Add(vm.WhenAnyValue(x => x.Loaded).Where(x => x).Subscribe(_ =>
            {
                if (Opened)
                {
                    ViewModel.Log().Info("GardenPlantTileView: plant loading ready, fading in plant " + ViewModel.Name);
                    FadeIn();
                }
            }));

            // this is only to take care of some cases where some plants
            // fail to fire the Opened event for some reason for user's own plant photos
            subs.Add(vm.WhenAnyValue(x => x.HasWriteAccess).Where(x => x)
                .Throttle(TimeSpan.FromMilliseconds(1000))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
            {
                ViewModel.Log().Info("GardenPlantTileView: plant has writeaccess, fading in plant " + ViewModel.Name);
                FadeIn();
            }));


            //subs.Dispose();
            //subs = Observable.CombineLatest(
            //    vm.WhenAnyValue(x => x.ShowPlaceHolder).Where(x => x),
            //    vm.WhenAnyValue(z => z.Loaded).Where(z => z && Opened),
            //    vm.WhenAnyValue(y => y.HasWriteAccess).Where(y => y))

            //    .Take(1).Subscribe(_ =>
            //{
            //    ViewModel.Log().Info("GardenPlantTileView: in combined observable for " + ViewModel.Name);

            //    if (vm.ShowPlaceHolder)
            //    {
            //        _FadeIn();
            //    }
            //    else if (vm.Loaded)
            //    {
            //        if (Opened)
            //        {
            //            ViewModel.Log().Info("GardenPlantTileView: plant loading ready, fading in plant " + ViewModel.Name);
            //            _FadeIn();
            //        }
            //        else
            //        {
            //            ViewModel.Log().Info("GardenPlantTileView: plant loaded but not opened yet " + ViewModel.Name);
            //        }
            //    }
            //    else if (vm.HasWriteAccess)
            //    {
            //        ViewModel.Log().Info("GardenPlantTileView: plant has writeaccess, fading in plant " + ViewModel.Name);
            //        _FadeIn();
            //    }
            //});
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
            ViewModel.Log().Info("disposing subs " + ViewModel.Name);
            DisposeSubs();
            //subs.Dispose();
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
            DisposeSubs(); 
        }


        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (Opened)
            {
                if (trexStoryboard != null)
                {
                    trexStoryboard.Begin();
                }
            }
        }

        private void RadContextMenu_Opened(object sender, EventArgs e)
        {
            MainWindowBase.ContextMenuOpen = true;
        }

        private void RadContextMenu_Closed(object sender, EventArgs e)
        {
            MainWindowBase.ContextMenuOpen = false;
        }
 

    }


}