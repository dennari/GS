



using ReactiveUI;
using MyMobileSample.Model.ViewModels;
using System.Windows.Controls;
using System.Windows;
using MyMobileSample.UI.ViewModels;

namespace MyMobileSample.UI.Views
{
    public partial class Page1 : UserControl, IViewFor<Page1ViewModel>
    {
        public Page1()
        {
            InitializeComponent();
            //this.OneWayBind(ViewModel, x => x.UrlPathSegment, x => x.Title.Text);
        }

        public Page1ViewModel ViewModel
        {
            get { return (Page1ViewModel)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);

                this.DataContext = value;
            }
        }


        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(Page1), new PropertyMetadata(null));



        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (Page1ViewModel)value; } }

    }


}