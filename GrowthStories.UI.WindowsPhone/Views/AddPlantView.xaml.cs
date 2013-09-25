﻿using System;
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
using System.Windows.Input;

namespace Growthstories.UI.WindowsPhone
{
    public partial class AddPlantView : UserControl, IViewFor<ClientAddPlantViewModel>
    {

        public AddPlantView()
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


        public ClientAddPlantViewModel ViewModel
        {
            get { return (ClientAddPlantViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(AddPlantView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (ClientAddPlantViewModel)value; } }

        private void TagBox_IconTapped(object sender, EventArgs e)
        {

            var text = TagBox.Text;

            if (!string.IsNullOrWhiteSpace(text))
            {
                this.ViewModel.AddTag.Execute(text);
                TagBox.Text = null;
                this.Focus();

            }

        }

        private void TagBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.TagBox_IconTapped(sender, e);
            }
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var s = sender as TextBlock;
            if (s != null && !string.IsNullOrWhiteSpace(s.Text))
            {
                this.ViewModel.RemoveTag.Execute(s.Text);
            }
        }
    }


}