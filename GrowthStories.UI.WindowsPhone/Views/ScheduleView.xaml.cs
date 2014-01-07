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
using Telerik.Windows.Controls;

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
            RadTimeSpanPicker p = this.TimeSpanPicker;
            p.PopupDefaultValue = TimeSpan.FromDays(2);
            p.MaxValue = TimeSpan.FromDays(365);
            p.MinValue = TimeSpan.FromHours(1);

        }
    }


}