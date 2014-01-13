using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows.Media;


namespace Growthstories.UI.WindowsPhone
{


    public class PlantActionAddEditViewBase : GSView<IPlantActionViewModel>
    {

    }


    public partial class PlantActionAddEditView : PlantActionAddEditViewBase
    {


        public PlantActionAddEditView()
        {
            InitializeComponent();
        }


        protected override void OnViewModelChanged(IPlantActionViewModel vm)
        {
            base.OnViewModelChanged(vm);
        }

        
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //App.RootFrame.RenderTransform = new CompositeTransform();
            SIPHelper.SIPGotVisible(SIPPlaceHolder);
        }


        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SIPHelper.SIPGotHidden(SIPPlaceHolder);
        }

        

    }
}