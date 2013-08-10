



using ReactiveUI;
using MyMobileSample.Model.ViewModels;
using System.Windows.Controls;
using System.Windows;
using MyMobileSample.UI.ViewModels;

namespace MyMobileSample.UI.Views
{
    public partial class Page2 : UserControl, IViewFor<Page2ViewModel>
    {
        public Page2()
        {
            InitializeComponent();
        }
        public Page2ViewModel ViewModel
        {
            get { return (Page2ViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }



        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(Page2), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (Page2ViewModel)value; this.DataContext = (Page2ViewModel)value; } }

    }
}