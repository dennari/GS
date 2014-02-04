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
        // maybe just use the viewmodel's Log() extension method?
        //private static ILog Logger = LogFactory.BuildLogger(typeof(SearchUsersViewModel));


        public PlantView()
        {
            InitializeComponent();

            if (Height != Double.NaN)
            {
                Height = Double.NaN;
            }
        }


        protected override void OnViewModelChanged(IPlantViewModel vm)
        {

            if (vm.HasWriteAccess)
            {
                Margin = new Thickness(0, 0, 0, 72);

            }
            else
            {
                Margin = new Thickness(0, 0, 0, 0);
            }


            vm.FilteredActions
                .ItemsAdded
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
            {
                this.ViewModel.Log().Info("itemadded, possibly triggering scroll");
                try
                {
                    if (TimeLine.ItemsSource.Count > 2)
                    {
                        if (TimeLine.ViewPort != null)
                        {
                            vm.Log().Info("scrolling");
                            TimeLine.ScrollTo(x);
                            TimeLine.ViewPort.SetViewportOrigin(new Point(0, 0));
                        }
                    }
                }
                catch { }
            });

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

            }
            catch
            {

            }
        }



    }


}