using System;

using Growthstories.UI.ViewModel;
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