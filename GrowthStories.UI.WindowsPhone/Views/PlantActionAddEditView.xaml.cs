﻿using System;
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



        private void GSViewGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Little bit hacky, but simple and more efficient than
            // listening to changes 
            //
            var mvm = ViewModel as PlantMeasureViewModel;
            if (mvm != null)
            {
                mvm.UpdatePreviousMeasurement();
            }

        }


        private void GSChatTextBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // hack to prevent blank space above SIP
            //
            // the blank space does not appear if we are scrolled down 
            // before touching the SIP, and this event handler catches 
            // the event early enough to produce the same effect
            //
            // drawback is a little bit ugly transition to SIP mode
            //
            //  -- JOJ 16.1.2014
            var sv = GSViewUtils.FindParent<ScrollViewer>((DependencyObject)sender);
            sv.ScrollToVerticalOffset(800);
        }

        private void GSChatTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            GSChatTextBox box = sender as GSChatTextBox;
            if (box == null)
                return;


            box.SetHintVisible(string.IsNullOrWhiteSpace(box.Text));

        }



    }
}