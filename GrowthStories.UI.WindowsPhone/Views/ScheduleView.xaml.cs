using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Microsoft.Phone.Tasks;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using Growthstories.UI.WindowsPhone.ViewModels;
using System.Reactive.Disposables;

namespace Growthstories.UI.WindowsPhone
{
    public partial class ScheduleView : UserControl, IViewFor<ScheduleViewModel>
    {

        public ScheduleView()
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
                return Disposable.Empty;
            });
        }


        public ScheduleViewModel ViewModel
        {
            get { return (ScheduleViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(ScheduleView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (ScheduleViewModel)value; } }

        private void ValueTypePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                this.ViewModel.SelectValueType.Execute(e.AddedItems[0]);
        }
    }


}