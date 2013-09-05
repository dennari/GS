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

namespace GrowthStories.UI.WindowsPhone
{
    public partial class MainView : PhoneApplicationPage
    {


        public MainView()
        {
            InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (this.DataContext != null)
            {
                var vm = this.DataContext as MainViewModel;
                if (vm != null)
                {
                    vm.OnNavigatedTo();
                }

            }

        }

        /// <summary>
        /// Defers back treatment to active pivot function
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (this.DataContext != null)
            {
                var vm = this.DataContext as MainViewModel;
                if (vm != null)
                {
                    vm.OnBackKeyPress(e);
                }

            }
        }

    }
}