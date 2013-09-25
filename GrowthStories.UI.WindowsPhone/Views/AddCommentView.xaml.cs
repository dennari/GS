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
using Growthstories.UI.WindowsPhone;
using Growthstories.UI.WindowsPhone.ViewModels;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{
    public partial class AddCommentView : UserControl, IViewFor<PlantCommentViewModel>
    {
        public AddCommentView()
        {
            InitializeComponent();

        }

        public PlantCommentViewModel ViewModel
        {
            get { return (PlantCommentViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                    this.DataContext = value;
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(AddCommentView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (PlantCommentViewModel)value; } }

    }
}