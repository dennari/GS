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
using BindableApplicationBar;
using ReactiveUI;
using System.Reactive.Disposables;
//using Growthstories.UI.WindowsPhone.ViewModels;
using Growthstories.UI.ViewModel;
//using Growthstories.UI.ViewModel;
//using Growthstories.UI.ViewModel;

namespace Growthstories.UI.WindowsPhone
{
    public partial class MainWindow : PhoneApplicationPage, IViewFor<Growthstories.UI.WindowsPhone.ViewModels.AppViewModel>
    {


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this.ViewModel;

            //this.ViewModel.Router.Navigate.Execute(RxApp.DependencyResolver.GetService<IMainViewModel>())


        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            IGSRoutableViewModel initialVM = null;
            IDictionary<string, string> qs = this.NavigationContext.QueryString;
            if (qs.ContainsKey("plant"))
            {
                try
                {
                    var id = Guid.Parse(qs["plant"]);
                    initialVM = this.ViewModel.PlantFactory(id, null);
                }
                catch (Exception)
                {

                }
            }
            if (initialVM == null)
                initialVM = this.ViewModel.Resolver.GetService<IMainViewModel>();
            this.ViewModel.Router.Navigate.Execute(initialVM);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit?",
            //                        MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            //{
            base.OnBackKeyPress(e);
            if (!ViewModel.Router.NavigateBack.CanExecute(null)) return;

            e.Cancel = true;
            ViewModel.Router.NavigateBack.Execute(null);
            //e.Cancel = true;

            //}
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
            //var cvm = ViewModel.Router.GetCurrentViewModel() as Growthstories.UI.ViewModel.IControlsPageOrientation;
            //if (cvm != null)
            //{
            ViewModel.PageOrientationChangedCommand.Execute((Growthstories.UI.ViewModel.PageOrientation)e.Orientation);
            //}

        }


        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    base.OnNavigatedTo(e);
        //    if (this.DataContext != null)
        //    {
        //        var vm = this.DataContext as MainViewModel;
        //        if (vm != null)
        //        {
        //            vm.OnNavigatedTo();
        //        }

        //    }

        //}

        ///// <summary>
        ///// Defers back treatment to active pivot function
        ///// </summary>
        ///// <param name="e"></param>
        //protected override void OnBackKeyPress(CancelEventArgs e)
        //{
        //    if (this.DataContext != null)
        //    {
        //        var vm = this.DataContext as MainViewModel;
        //        if (vm != null)
        //        {
        //            vm.OnBackKeyPress(e);
        //        }

        //    }
        //}

        protected Growthstories.UI.WindowsPhone.ViewModels.AppViewModel _ViewModel;
        public Growthstories.UI.WindowsPhone.ViewModels.AppViewModel ViewModel
        {
            get { return _ViewModel ?? (_ViewModel = new Growthstories.UI.WindowsPhone.ViewModels.AppViewModel()); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(Growthstories.UI.WindowsPhone.ViewModels.AppViewModel), typeof(MainWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set
            {

                var vm = (Growthstories.UI.WindowsPhone.ViewModels.AppViewModel)value;
                if (vm != null)
                {
                    this.ViewModel = vm;
                    this.DataContext = vm;
                }
            }
        }
    }
}