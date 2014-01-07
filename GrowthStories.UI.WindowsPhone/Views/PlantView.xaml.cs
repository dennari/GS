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

namespace Growthstories.UI.WindowsPhone
{

    public class PlantViewBase : GSView<IPlantViewModel>
    {

    }


    public partial class PlantView : PlantViewBase
    {
        
        public PlantView()
        {
            InitializeComponent();

            if (Height != Double.NaN)
            {
                Height = Double.NaN;
            }
            
            if (ViewModel != null)
            {
                OnViewModelChanged(ViewModel);
            }
        }


        IDisposable PinCommandSubscription = Disposable.Empty;
        IDisposable ShareCommandSubscription = Disposable.Empty;
        IDisposable DeleteCommandSubscription = Disposable.Empty;


        protected override void OnViewModelChanged(IPlantViewModel vm)
        {

            PinCommandSubscription.Dispose();
            ViewModel.HasTile = GSTileUtils.GetShellTile(vm) != null;
            
            PinCommandSubscription = vm.PinCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                if (vm.HasTile == null) {
                    CreateOrUpdateTile();
                } else {
                    DeleteTile();
                }
            });

            ShareCommandSubscription.Dispose();
            ShareCommandSubscription = vm.ShareCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                Share(vm);
            });

            if (vm.UserId == vm.App.User.Id) {
                Margin = new Thickness(0, 0, 0, 72);
            } else {
                Margin = new Thickness(0, 0, 0, 0);
            }

            DeleteCommandSubscription.Dispose();
            DeleteCommandSubscription = vm.DeleteCommand
                .ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                DeleteTile();
            });
        }


        private void Share(IPlantViewModel vm)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();

            shareLinkTask.Title = "Story of " + vm.Name;
            shareLinkTask.LinkUri = new Uri(
                "http://www.growthstories.com/plant/" + vm.UserId + "/" + vm.Id , UriKind.Absolute);
            shareLinkTask.Message = "Check out how my plant " + vm.Name + " is doing!";

            shareLinkTask.Show();
        }


        private void DeleteTile()
        {
            GSTileUtils.DeleteTile(ViewModel);
        }


        private void CreateOrUpdateTile()
        {         
            GSTileUtils.CreateOrUpdateTile(ViewModel);
        }


        private static HashSet<object> LoadedImages = new HashSet<object>();
        private static HashSet<object> LoadedSources = new HashSet<object>();
        private HashSet<object> AnimatedSources = new HashSet<object>();

     
        private void ImageBrush_ImageOpened(object sender, RoutedEventArgs e)
        {            
            var img = sender as System.Windows.Controls.Image;

            LoadedImages.Add(img);
            LoadedSources.Add(img.Source);

            FadeInImage(img);
        }


        private void FadeInImage(Image img)
        {
            AnimatedSources.Add(img.Source);

            var ha = new DoubleAnimation();
            ha.Duration = new Duration(TimeSpan.FromSeconds(0.7));
            ha.From = 0;
            ha.To = 220;
            ha.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            var oa = new DoubleAnimation();
            oa.Duration = new Duration(TimeSpan.FromSeconds(0.7));
            oa.From = 0.0;
            oa.To = 1.0;
            oa.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            Storyboard sb = new Storyboard();
            sb.Children.Add(oa);
            sb.Children.Add(ha);

            var b = GSViewUtils.FindParent<Button>(img);

            if (b != null)
            {
                Storyboard.SetTarget(ha, b);
                Storyboard.SetTargetProperty(ha, new PropertyPath("Height"));

                Storyboard.SetTarget(oa, b);
                Storyboard.SetTargetProperty(oa, new PropertyPath("Opacity"));
            }

            //if (b.Opacity == 0.0)
            //{
                sb.Begin();
            //}    
        }


        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            
            var img = sender as System.Windows.Controls.Image;
            var b = GSViewUtils.FindParent<Button>(img);

            bool contains = LoadedImages.Contains(img);
            bool opaCheck = b.Opacity == 0.0;
            bool c2 = LoadedSources.Contains(img.Source);
            
            // the longlist selector is lazy loading content all the time
            // and each the Image object is a new one 
            // 
            // we get the ImageOpened event only once during application running for each source
            // we get the ImageLoaded event each time the long list selector is doing some lazy
            //   loading
            //
            if (!LoadedImages.Contains(img) && LoadedSources.Contains(img.Source))
            {
                if (AnimatedSources.Contains(img.Source))
                {
                    if ((int)b.Height != 220)
                    {
                        b.Height = 220;
                        b.Opacity = 1.0;
                    }
                
                } else {
                    FadeInImage(img);
                }
            }
        }


        //private void PlantActionView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    var plantActionView = (PlantActionView)sender;
        //    //var plant = ViewModel.SelectedItem;
        //    ViewModel.ActionTapped.Execute(plantActionView.ViewModel);
        //}
    }





}