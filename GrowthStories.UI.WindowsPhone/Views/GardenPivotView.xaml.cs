using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System.Reactive.Linq;
using EventStore.Logging;


namespace Growthstories.UI.WindowsPhone
{


    public class GardenPivotViewBase : GSView<IGardenPivotViewModel>
    {

    }


    public partial class GardenPivotView : GardenPivotViewBase
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(GardenPivotView));


        static ReactiveCommand Constructed = new ReactiveCommand();
        

        public GardenPivotView()
        {
            InitializeComponent();

            Logger.Info("initializing new gardenpivotview");

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

    }
}