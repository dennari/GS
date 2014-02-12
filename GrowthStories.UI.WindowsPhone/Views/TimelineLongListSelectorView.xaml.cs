using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Growthstories.UI.ViewModel;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Reactive.Disposables;
using ReactiveUI;
using System.Reactive.Linq;

namespace Growthstories.UI.WindowsPhone
{


    public class TimelineLongListSelectorViewBase : GSView<IPlantViewModel>
    {

    }


    public partial class TimelineLongListSelectorView : TimelineLongListSelectorViewBase
    {

        public TimelineLongListSelectorView()
        {
            InitializeComponent();
        }

        IDisposable subs = Disposable.Empty;

        protected override void OnViewModelChanged(IPlantViewModel vm)
        {
            subs =
                vm.FilteredActions
                    .ItemsAdded
                    .Throttle(TimeSpan.FromMilliseconds(300))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                {
                    try
                    {

                        if (TimeLine.ItemsSource.Count > 2)
                        {
                            vm.Log().Info("scrolling");
                            TimeLine.ScrollTo(x);

                            //if (TimeLine.ViewPort != null)
                            //{                           
                            //TimeLine.ViewPort.SetViewportOrigin(new Point(0, 0));
                            //}
                        }
                    }
                    catch { }
                });
        }


        public void CleanUp()
        {
            subs.Dispose();
            TimeLine.ItemsSource = null;
            ViewHelpers.ClearLongListSelectorDependencyValues(TimeLine);
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
        

        ~TimelineLongListSelectorView()
        {
            NotifyDestroyed("");
        }


    }





}
