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


    public class TimelinePhotoViewBase : GSView<IPlantPhotographViewModel>
    {

    }


    public partial class TimelinePhotoView : TimelinePhotoViewBase
    {


        public TimelinePhotoView()
        {
            InitializeComponent();
            Height = Double.NaN;
        }


        protected override void OnViewModelChanged(IPlantPhotographViewModel vm)
        {
            ViewModel.Log().Info("onviewmodelchanged for timelinephotoview image " + vm.PlantActionId);
            ImageControl.CacheMode = null;
        }


        public static HashSet<Guid> ResetImages = new HashSet<Guid>();
        public static HashSet<Guid> OpenedImages = new HashSet<Guid>();
        public static HashSet<Guid> AnimatedImages = new HashSet<Guid>();


        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("imageopened for " + ViewModel.PlantActionId);

            var img = sender as System.Windows.Controls.Image;
            OpenedImages.Add(ViewModel.PlantActionId);

            FadeInImage();
        }


        private void Img_ImageFailed(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("imagefailed for " + ViewModel.PlantActionId);
            LoadingFailed.Visibility = Visibility.Visible;
            ButtonControl.IsHitTestVisible = false;
            LoadingPhoto.Visibility = Visibility.Collapsed;
            //ButtonControl.BorderThickness = new Thickness(3);
        }


        private void FadeInImage()
        {
            ViewModel.Log().Info("fading in image for " + ViewModel.PlantActionId);

            AnimatedImages.Add(ViewModel.PlantActionId);

            Storyboard sb = new Storyboard();

            //var ha = new DoubleAnimation();
            //ha.Duration = new Duration(TimeSpan.FromSeconds(0.7));
            //ha.From = 0;
            //ha.To = 220;
            //ha.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            //sb.Children.Add(ha);
            
            var oa = new DoubleAnimation();
            oa.Duration = new Duration(TimeSpan.FromSeconds(0.7));
            oa.From = 0.0;
            oa.To = 1.0;
            oa.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            sb.Children.Add(oa);

            //var b = ButtonControl;
            Storyboard.SetTarget(oa, ImageControl);
            Storyboard.SetTargetProperty(oa, new PropertyPath("Opacity"));
            sb.Begin();
            //b.BorderThickness = new Thickness(3);
        }


        private void Img_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("image unloaded event for " + ViewModel.PlantActionId);
        }


        // Show image (with animation if not already animated)
        //
        private void ShowImage()
        {
            ViewModel.Log().Info("showing image " + ViewModel.PlantActionId);

            LoadingFailed.Visibility = Visibility.Collapsed;
            LoadingPhoto.Visibility = Visibility.Collapsed;
            ButtonControl.IsHitTestVisible = true;

            //var b = ButtonControl;

            if (AnimatedImages.Contains(ViewModel.PlantActionId))
            {
                ImageControl.Opacity = 1.0;
                //b.Opacity = 1.0;
                //b.BorderThickness = new Thickness(3);

 //               if ((int)b.Height != 220)
  //              {
    //                //b.Height = 220;
      //              b.Opacity = 1.0;
        //            //b.BorderThickness = new Thickness(3);
          //      }
            }
            else
            {
                FadeInImage();
            }

        }


        private void Img_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("image loaded event for " + ViewModel.PlantActionId);

            var img = ImageControl;
            var b = ButtonControl;

            // the longlist selector is lazy loading content all the time
            // and each the Image object is a new one 
            //
            // we get the ImageOpened event only once during application running for each source
            // we get the ImageLoaded event each time the long list selector is doing some lazy
            //   loading
            //
            if (OpenedImages.Contains(ViewModel.PlantActionId))
            {
                ShowImage();
            }
        }


    }

}
