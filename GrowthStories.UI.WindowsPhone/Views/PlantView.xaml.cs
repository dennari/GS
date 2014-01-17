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


    public class PlantViewBase : GSView<IPlantViewModel>
    {

    }


    public partial class PlantView : PlantViewBase
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(SearchUsersViewModel));


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
        IDisposable DeleteRequestedCommandSubscription = Disposable.Empty;


        protected override void OnViewModelChanged(IPlantViewModel vm)
        {

            PinCommandSubscription.Dispose();
            ViewModel.HasTile = GSTileUtils.GetShellTile(vm) != null;

            PinCommandSubscription = vm.PinCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                if (!vm.HasTile)
                {
                    CreateOrUpdateTile();
                }
                else
                {
                    vm.App.DeleteTileCommand.Execute(vm);
                }
            });

            ShareCommandSubscription.Dispose();
            ShareCommandSubscription = vm.ShareCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                Share(vm);
            });

            if (vm.UserId == vm.App.User.Id)
            {
                Margin = new Thickness(0, 0, 0, 72);
            }
            else
            {
                Margin = new Thickness(0, 0, 0, 0);
            }

            DeleteCommandSubscription.Dispose();
            DeleteCommandSubscription = vm.DeleteCommand.Subscribe(_ =>
            {
                ViewModel.App.Router.NavigateBack.Execute(null);
            });

            vm.Actions.ItemsAdded.Subscribe(x =>
            {
                try
                {
                    if (TimeLine.ItemsSource.Count > 2)
                    {
                        TimeLine.ScrollTo(x);
                    }
                }
                catch { }
            });

        }


        private void Share(IPlantViewModel vm)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();

            shareLinkTask.Title = "Story of " + vm.Name;
            shareLinkTask.LinkUri = new Uri(
                "http://www.growthstories.com/plant/" + vm.UserId + "/" + vm.Id, UriKind.Absolute);
            shareLinkTask.Message = "Check out how my plant " + vm.Name + " is doing!";

            shareLinkTask.Show();
        }


        private void CreateOrUpdateTile()
        {
            GSMainProgramTileUtils.CreateOrUpdateTile(ViewModel);
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
            /*
            var ha = new DoubleAnimation();
            ha.Duration = new Duration(TimeSpan.FromSeconds(0.7));
            ha.From = 0;
            ha.To = 220;
            ha.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            */
            var oa = new DoubleAnimation();
            oa.Duration = new Duration(TimeSpan.FromSeconds(0.7));
            oa.From = 0.0;
            oa.To = 1.0;
            oa.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            Storyboard sb = new Storyboard();
            sb.Children.Add(oa);
            //sb.Children.Add(ha);

            var b = GSViewUtils.FindParent<Button>(img);

            if (b != null)
            {
                //Storyboard.SetTarget(ha, b);
                //Storyboard.SetTargetProperty(ha, new PropertyPath("Height"));

                Storyboard.SetTarget(oa, b);
                Storyboard.SetTargetProperty(oa, new PropertyPath("Opacity"));
            }

            //if (b.Opacity == 0.0)
            //{
            sb.Begin();
            b.Height = 220;
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

                }
                else
                {
                    FadeInImage(img);
                }
            }
        }


        private void TimeLine_Loaded(object sender, RoutedEventArgs e)
        {
            // hack to hide the scrollbar from longlistselector
            // needs to be done as it screws up alignment on the grid
            //
            // from http://stackoverflow.com/questions/18414498/hide-scrollbar-in-longlistselector
            //
            //   -- JOJ 17.1.2014

            try
            {
                if (TimeLine.ItemsSource.Count > 0)
                {
                    var sb = ((FrameworkElement)VisualTreeHelper.GetChild(TimeLine, 0)).FindName("VerticalScrollBar") as ScrollBar;
                    sb.Margin = new Thickness(0, 0, -10, 0);
                }
            
            } catch {
                
            }
        }



    }


}