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
using System.Windows.Media.Imaging;



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
            Img.CacheMode = null;
        }


        private IDisposable subscription;


        protected override void OnViewModelChanged(IPlantViewModel vm)
        {
            //ViewModel.Log().Info("GardenPlantTileView: onviewmodelchanged gardenplanttileview " + vm.Name);
            //ViewModel.Log().Info("GardenPlantTileView: plant loaded is " + vm.Loaded);
            //ViewModel.Log().Info("GardenPlantTileView: vw has writeaccess is " + vm.HasWriteAccess);

            if (subscription != null)
            {
                subscription.Dispose();
            }

            subscription = vm.WhenAnyValue(x => x.ShowPlaceHolder).Subscribe(x =>
            {
                if (x)
                {
                    //ViewModel.Log().Info("GardenPlantTileView: fading in placeholder for " + ViewModel.Name);
                    FadeIn();
                }
            });

            
            vm.WhenAnyValue(x => x.Loaded).Subscribe(x =>
            {
                if (Opened)
                {
                    //ViewModel.Log().Info("GardenPlantTileView: plant loading ready, fading in plant " + ViewModel.Name);
                    FadeIn();
                }

                if (!ViewModel.HasWriteAccess)
                {
                    //ViewModel.Log().Info("GardenPlantTileView: showing loading for garden plant tile " + ViewModel.Name);
                    LoadingPhoto.Visibility = Visibility.Visible;
                }
            });
            
        }


        private void ResetImage(Image i)
        {
            //ViewModel.Log().Info("GardenPlantTileView: resetting image");
            var bitmapImage = i.Source as BitmapImage;
           
            if (bitmapImage != null)
            {
                bitmapImage.UriSource = null;
                i.Source = null;
            }
        }

        private bool Opened = false;

        public static HashSet<Guid> OpenedImages = new HashSet<Guid>();


        // Real image (no placeholder) has been opened
        // 
        //
        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            OpenedImages.Add(this.ViewModel.Id);
            //ViewModel.Log().Info("GardenPlantTileView: image opened event for " + ViewModel.Name);
            Opened = true;
            if (ViewModel.Loaded)
            {
                FadeIn();
            }
        }

        

        // Fade the content in if not already faded/fading in
        //
        //
        private void FadeIn()
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
                //ViewModel.Log().Info("GardenPlantTileView: starting fadein for " + ViewModel.Name);
                sb.Begin();
            }
            else
            {
                //ViewModel.Log().Info("GardenPlantTileView: skipping fadein for " + ViewModel.Name);
            }
        }


        private void FadeInCompleted(object sender, EventArgs e)
        {
            //ViewModel.Log().Debug("fadein completed for {0}", ViewModel.Name);
            trexStoryboard.Begin();
        }


        private void Img_Loaded(object sender, RoutedEventArgs e)
        {
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
            //ViewModel.Log().Info("GardenPlantTileView: button tapped" + ViewModel.Name);

            if (ViewModel != null)
            {
                ViewModel.ShowDetailsCommand.Execute(ViewModel);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.Log().Info("GardenPlantTileView: button click" + ViewModel.Name);

        }

        private void RadContextMenu_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //ViewModel.Log().Info("GardenPlantTileView: radcontextmenu tap" + ViewModel.Name);
        }

    }


}