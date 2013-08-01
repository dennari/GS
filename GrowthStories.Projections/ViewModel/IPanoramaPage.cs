using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Growthstories.UI.ViewModel
{
    public interface IPanoramaPage
    {
        ObservableCollection<ButtonViewModel> AppBarButtons { get; }
    }
}
