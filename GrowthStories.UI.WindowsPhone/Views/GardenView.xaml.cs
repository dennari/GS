using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EventStore.Logging;
using Growthstories.UI.ViewModel;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{


    public class GardenViewBase : GSView<IGardenViewModel>
    {

    }


    public partial class GardenView : GardenViewBase
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(GardenView));


        public GardenView()
        {
            this.SetBinding(ViewModelProperty, new Binding());

            InitializeComponent();

            if (Height != Double.NaN)
            {
                Height = Double.NaN;
            }

            if (OwnGarden == null || OwnGarden.Equals("TRUE"))
            {
                Logger.Info("owngardenplaceholder to visible");
                OwnGardenPlaceHolder.Visibility = Visibility.Visible;
            }
            else
            {
                Logger.Info("nonowngardenplaceholder to visible");
                NonOwnGardenPlaceHolder.Visibility = Visibility.Visible;
            }

            Logger.Info("initialized garden view");
        }


        public static readonly DependencyProperty CleanUpOnUnloadProperty =
            DependencyProperty.Register("CleanUpOnUnload", typeof(string), typeof(GardenView), new PropertyMetadata("FALSE"));


        public static readonly DependencyProperty MainScrollerHeightProperty =
            DependencyProperty.Register("MainScrollerHeight", typeof(int), typeof(GardenView), new PropertyMetadata(480));




        public string CleanUpOnUnload
        {
            get
            {
                return (string)GetValue(CleanUpOnUnloadProperty);
            }
            set
            {
                SetValue(CleanUpOnUnloadProperty, value);
            }
        }


        public int MainScrollerHeight
        {
            get
            {
                return (int)GetValue(MainScrollerHeightProperty);
            }
            set
            {
                SetValue(MainScrollerHeightProperty, value);
            }
        }


        public static readonly DependencyProperty OwnGardenProperty =
             DependencyProperty.Register("OwnGarden", typeof(string), typeof(GardenView), new PropertyMetadata("TRUE"));

        public string OwnGarden
        {
            get
            {
                return (string)GetValue(OwnGardenProperty);
            }
            set
            {
                SetValue(OwnGardenProperty, value);
            }
        }


        protected override void OnViewModelChanged(IGardenViewModel vm)
        {
            if (vm == null)
                return;

            vm.Log().Info("settings mainscroller height to {0}", MainScrollerHeight);
            MainScroller.Height = MainScrollerHeight;

            OnceLoadedContainer.Visibility = Visibility.Collapsed;
            BusyIndicator.Visibility = Visibility.Visible;
            BusyIndicator.IsRunning = true;

            var gvm = vm as GardenViewModel;
            gvm.WhenAnyValue(x => x.IsLoaded).Where(x => x).Take(1).Subscribe(_ =>
            {
                OnceLoadedContainer.Visibility = Visibility.Visible;
                BusyIndicator.Visibility = Visibility.Collapsed;
                BusyIndicator.IsRunning = false;
            });

        }


        private void PlantsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.SelectedItemsChanged.Execute(Tuple.Create(e.AddedItems, e.RemovedItems));
        }


        private PlantViewModel GetViewModel(object sender)
        {
            var img = sender as System.Windows.Controls.Image;
            var c4fTile = GSViewUtils.FindParent<Button>(img);
            var button = GSViewUtils.FindParent<Button>(c4fTile);

            return button.CommandParameter as PlantViewModel;
        }


        private void Button_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // for some reason command triggering did not work for the button
            // so we are doing it this way
            //   -- JOJ 5.12.2014

            var btn = sender as Button;
            if (ViewModel != null)
            {
                ViewModel.ShowDetailsCommand.Execute(btn.CommandParameter);
            }
        }

        private void ViewRoot_Unloaded(object sender, RoutedEventArgs e)
        {

            Logger.Info("gardenview unloaded for {0}, CleanUpOnUnload is {1}", ViewModel.Username, CleanUpOnUnload);
            if (CleanUpOnUnload != null && CleanUpOnUnload.Equals("TRUE"))
            {
                Logger.Info("cleaning up gardenview {0}", ViewModel.Username);
                PlantsSelector.IsSelectionEnabled = false;
                PlantsSelector.ItemsSource = null;
                PlantsSelector.SelectionChanged -= PlantsSelector_SelectionChanged;

                ViewHelpers.ClearLongListMultiSelectorDependencyValues(PlantsSelector);
            }
        }


        ~GardenView()
        {
            NotifyDestroyed("");
        }

    }
}