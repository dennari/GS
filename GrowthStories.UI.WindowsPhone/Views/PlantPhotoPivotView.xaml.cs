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
using Growthstories.UI.ViewModel;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{

    public class PlantPhotoPivotViewBase : GSView<IPlantViewModel>
    {

    }


    public partial class PlantPhotoPivotView : PlantPhotoPivotViewBase
    {

        public PlantPhotoPivotView()
        {            
            //((ContentControl)Parent).Height = Double.NaN;
            
            InitializeComponent();
            Height = Double.NaN;

            //MainWindow.instance.Height = Double.NaN;

            //((ContentControl)Parent).Height = Double.NaN;
        }

        

    }
}