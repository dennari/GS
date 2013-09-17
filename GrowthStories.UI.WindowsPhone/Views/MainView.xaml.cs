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
using Growthstories.UI.ViewModel;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{
    public partial class MainView : UserControl, IViewFor<MainViewModel>
    {


        public MainView()
        {
            InitializeComponent();

        }

        public MainViewModel ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                    this.DataContext = value;
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(MainView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (MainViewModel)value; } }

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

    }
}