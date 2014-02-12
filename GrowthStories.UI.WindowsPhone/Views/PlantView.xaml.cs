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

        private static ILog Logger = LogFactory.BuildLogger(typeof(PlantView));


        public PlantView()
        {
            Logger.Info("initializing new plantview");
            InitializeComponent();

            if (Height != Double.NaN)
            {
                Height = Double.NaN;
            }
        }

        private IDisposable subs;


        protected override void OnViewModelChanged(IPlantViewModel vm)
        {
            Logger.Info("onviewmodelchanged for {0}", vm.Name);
        
            if (vm.HasWriteAccess)
            {
                Margin = new Thickness(0, 0, 0, 72);

            }
            else
            {
                Margin = new Thickness(0, 0, 0, 0);
            }

            //vm.FilteredActions
            //    .ItemsAdded
            //    .Throttle(TimeSpan.FromMilliseconds(300))
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(x =>
            //{
            //    this.ViewModel.Log().Info("itemadded, possibly triggering scroll");
            //    try
            //    {

            //        if (TimeLine.ItemsSource.Count > 2)
            //        {
            //            vm.Log().Info("scrolling");
            //            TimeLine.ScrollTo(x);

            //            //if (TimeLine.ViewPort != null)
            //            //{                           
            //            //TimeLine.ViewPort.SetViewportOrigin(new Point(0, 0));
            //            //}
            //        }
            //    }
            //    catch { }
            //});
           
            subs = vm.WhenAnyValue(x => x.ShouldBeFullyLoaded).Subscribe(x =>
            {
                if (x)
                {
                    _AddLongListSelector();
                }
                else
                {
                    _RemoveLongListSelector();
                }
            });

            // when selected plant is no more one of this user, clean up subscription
            vm.DifferentUsersPlantSelected.Take(1).Subscribe(x =>
            {
                ViewModel.Log().Info("cleaning up plantview for {0}", ViewModel.Name);
                subs.Dispose();
                _RemoveLongListSelector();
            });
        }




        private void _RemoveLongListSelector()
        {
            ViewModel.Log().Info("removing longlistselector for {0}", ViewModel.Name);
            foreach (var c in TimelineContainer.Children)
            {
                var lls = c as TimelineLongListSelectorView;
                if (lls != null)
                {
                    lls.CleanUp();
                }
            }
            TimelineContainer.Children.Clear();
        }
    

        private void TimelineContainer_Unloaded(object sender, RoutedEventArgs e)
        {
            //_RemoveLongListSelector();
        }


        private void _AddLongListSelector()
        {
            var lls = new TimelineLongListSelectorView();
            lls.ViewModel = this.ViewModel;

            if (TimelineContainer.Children.Count() == 0)
            {
                ViewModel.Log().Info("adding longlistselector for {0}", this.ViewModel.Name);
                TimelineContainer.Children.Add(lls);
            }
        }


        private void TimelineContainer_Loaded(object sender, RoutedEventArgs e)
        {
            //ViewModel.Log().Info("loaded timelinecontainer for {0}", ViewModel.Name);
            //_AddLongListSelector();
        }


        ~PlantView()
        {
            NotifyDestroyed("");
        }

    }




}