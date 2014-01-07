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
using ReactiveUI;
using Growthstories.UI.WindowsPhone.ViewModels;
using System.ComponentModel;

namespace Growthstories.UI.WindowsPhone
{

    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor




        public MainPage()
        {
            InitializeComponent();

            this.ViewModel = new SimpleAppViewModel();

        }


        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register("ViewModel", typeof(AppViewModel), typeof(MainPage), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));

        public AppViewModel ViewModel
        {
            get
            {
                return (AppViewModel)GetValue(ViewModelProperty);
            }
            set
            {
                if (value != null)
                    SetValue(ViewModelProperty, value);
            }
        }




        /// <summary>
        /// We get here on the initial load AND whenever we resume, i.e. from tasks
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            //IDictionary<string, string> qs = this.NavigationContext.QueryString;
            //Exception ee = null;
            //if (qs.Count > 0)
            //    try
            //    {
            //        this.NavigateWithDeepLink(qs);
            //        return;
            //    }
            //    catch (Exception E)
            //    {
            //        ee = E;
            //    }

            //if (ee != null || this.ViewModel.Router.NavigationStack.Count == 0) // don't do anything if this isn't the initial load
            //    this.ViewModel.Router.Navigate.Execute(new MainViewModel(this.ViewModel));

        }

        protected void NavigateWithDeepLink(IDictionary<string, string> qs)
        {

            var id = Guid.Parse(qs["plant"]);
            //this.ViewModel.Router.Navigate.Execute(this.ViewModel.PlantFactory(id));

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
            //ViewModel.PageOrientationChangedCommand.Execute((Growthstories.UI.ViewModel.PageOrientation)e.Orientation);

            //}

            if (e.Orientation == Microsoft.Phone.Controls.PageOrientation.LandscapeLeft || e.Orientation == Microsoft.Phone.Controls.PageOrientation.LandscapeRight)
            {
                this.ViewModel.Router.Navigate.Execute(new YAxisShitViewModel(null, this.ViewModel));
            }
            else
            {
                this.ViewModel.Router.NavigateBack.Execute(null);
            }



        }







        public void ViewModelChanged(object vm)
        {
            // throw new NotImplementedException();
        }
    }
}