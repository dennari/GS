
using MyMobileSample.UI.Resources;
using ReactiveUI;
using MyMobileSample.UI.ViewModels;
using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Reactive;
using System.Reactive.Linq;
using MyMobileSample.Model.ViewModels;
using System.Collections;

namespace MyMobileSample.UI
{
    public partial class MainPage : PhoneApplicationPage, IViewFor<AppViewModel>
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();


        }


        public AppViewModel ViewModel
        {
            get { return (AppViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AppViewModel), typeof(MainPage), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set
            {

                ViewModel = (AppViewModel)value;
            }
        }
    }
}