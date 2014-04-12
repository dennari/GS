using System;
using System.Linq;
using System.Reactive.Linq;
using EventStore.Logging;
using Growthstories.UI.ViewModel;
using Growthstories.Domain;
using ReactiveUI;
using System.Diagnostics;


namespace Growthstories.UI.WindowsPhone
{


    public class GardenPivotViewBase : GSView<IGardenPivotViewModel>
    {

    }


    public partial class GardenPivotView : GardenPivotViewBase
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(GardenPivotView));


        static ReactiveCommand Constructed = new ReactiveCommand();

        private ReactiveList<string> LogItems;
        public GardenPivotView()
        {
            InitializeComponent();

            Logger.Info("initializing new gardenpivotview");


            //Logg.ItemsSource = LogItems;
            //Logg.ItemRealized += (s, e) =>
            //{
            //    //e.Container.
            //};
            //Plants.Unloaded += (s, e) =>
            //{
            //    LogItemsAdd(string.Format("Unloaded whole Pivot"));

            //};
            //Plants.Loaded += (s, e) =>
            //{
            //    LogItemsAdd(string.Format("Loaded whole Pivot"));

            //};

            //Plants.LoadingPivotItem += (s, e) =>
            //{
            //    var plant = e.Item.DataContext as IPlantViewModel;
            //    if (plant != null)
            //        LogItemsAdd(string.Format("Loading {0}", plant.Name));

            //    //e.Item.Content.
            //};
            //Plants.LoadedPivotItem += (s, e) =>
            //{
            //    var plant = e.Item.DataContext as IPlantViewModel;
            //    //e.Item.ContentTemplate.LoadContent();
            //    if (plant != null)
            //    {
            //        var item = string.Format("Loaded {0}", plant.Name);
            //        LogItemsAdd(item);

            //    }


            //};
            //Plants.UnloadingPivotItem += (s, e) =>
            //{
            //    var plant = e.Item.DataContext as IPlantViewModel;
            //    if (plant != null)
            //        LogItemsAdd(string.Format("Unloading {0}", plant.Name));


            //};
            //Plants.UnloadedPivotItem += (s, e) =>
            //{
            //    var plant = e.Item.DataContext as IPlantViewModel;
            //    if (plant != null)
            //    {
            //        var item = string.Format("Unloaded {0}", plant.Name);
            //        LogItemsAdd(item);
            //    }


            //};


            this.WhenAnyValue(x => x.ViewModel.Plants)
                .Where(x => x != null)
                .Throttle(TimeSpan.FromMilliseconds(300), RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
            {

                try
                {
                    this.Plants.ItemsSource = null;
                    this.Plants.ItemsSource = this.ViewModel.Plants.ToArray();
                }
                catch (Exception e)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    ViewModel.Log().DebugExceptionExtended("Refreshing Plants.ItemsSource threw exception", e);
                }

            });

            this.WhenAnyObservable(x => x.ViewModel.Plants.CountChanged).Subscribe(x =>
            {
                this.Plants.ItemsSource = null;
                this.Plants.ItemsSource = this.ViewModel.Plants.ToArray();
            });

            Constructed.Execute(null);
            Constructed.Take(1).Subscribe(_ => CleanUp());
        }



        protected override void OnViewModelChanged(IGardenPivotViewModel vm)
        {
            //base.OnViewModelChanged(vm);
            vm.Log().Info("GardenPivotView: OnViewModelChanged");





        }


        public void CleanUp()
        {
            ViewModel.Log().Info("cleaning up gardenpivotview {0}", ViewModel.Username);
            //Plants.SelectedItem = null;
            Plants.ItemsSource = null;
            ViewHelpers.ClearPivotDependencyValues(Plants);
            ViewHost.CleanUp();
            //LayoutRoot.Children.Clear();
            //
            //foreach (var p in ViewModel.Plants)
            //{
            //    p.ShouldBeFullyLoaded = false;
            //}
        }


        ~GardenPivotView()
        {
            NotifyDestroyed("");
        }

        //private void PlantView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    var p = sender as PlantView;
        //    LogItemsAdd(string.Format("Loaded PlantView for plant {0}", p.ViewModel.Name));
        //}

        //private void PlantView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    var p = sender as PlantView;
        //    LogItemsAdd(string.Format("Unloaded PlantView for plant {0}", p.ViewModel.Name));
        //}

        //private void LogItemsAdd(string p)
        //{
        //    var item = string.Format(p + ", {0:HH:mm:ss.fff}", DateTime.Now);
        //    this.LogItems.Add(item);
        //    //this.Logg.ScrollTo(item);
        //    //this.Logg.ViewPort.
        //}

    }
}