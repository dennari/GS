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
    public class ScheduleViewBase : GSView<IScheduleViewModel>
    {

    }

    public partial class ScheduleView : ScheduleViewBase
    {

        public ScheduleView()
        {
            InitializeComponent();

        }


        private void ValueTypePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                this.ViewModel.SelectValueType.Execute(e.AddedItems[0]);
        }
    }


}