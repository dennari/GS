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
using Growthstories.UI.WindowsPhone;
using ReactiveUI;
using System.Reactive.Disposables;
using Growthstories.UI.WindowsPhone.ViewModels;

namespace Growthstories.UI.WindowsPhone
{
    public partial class AddPhotographView : UserControl, IViewFor<ClientAddPhotographViewModel>
    {
        public AddPhotographView()
        {
            InitializeComponent();
            this.WhenNavigatedTo(ViewModel, () =>
            {
                /* COOLSTUFF: Setting up the View
                 * 
                 * Whenever we're Navigated to, we want to set up some bindings.
                 * In particular, we want to Subscribe to the HelloWorld command
                 * and whenever the ViewModel invokes it, we will pop up a 
                 * Message Box.
                 */

                // Make XAML Bindings be relative to our ViewModel
                DataContext = ViewModel;
                ViewModel.Chooser.Show();
                return Disposable.Empty;
            });

        }

        public ClientAddPhotographViewModel ViewModel
        {
            get { return (ClientAddPhotographViewModel)GetValue(ViewModelProperty); }
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
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(AddPhotographView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (ClientAddPhotographViewModel)value; } }

    }
}