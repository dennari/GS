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
using System.Windows.Input;
using Growthstories.Sync;
using System.Reactive.Disposables;

namespace Growthstories.UI.WindowsPhone
{
    public class PlantActionListViewBase : GSView<IPlantActionListViewModel>
    {

    }

    public partial class PlantActionListView : PlantActionListViewBase
    {

        public PlantActionListView()
        {
            InitializeComponent();
            //this.ViewModel = new PlantActionListViewModel();
        }


    }
}