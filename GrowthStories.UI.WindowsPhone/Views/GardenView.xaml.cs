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
using System.Windows.Data;

namespace Growthstories.UI.WindowsPhone
{
    public partial class GardenView : UserControl, IViewFor<IGardenViewModel>, IReactsToViewModelChange
    {

        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (IGardenViewModel)value; } }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IGardenViewModel), typeof(GardenView), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));




        public IGardenViewModel ViewModel
        {
            get { return (IGardenViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                }
            }
        }

        public GardenView()
        {
            InitializeComponent();
            //this.SetBinding(ViewModelProperty, new Binding());
        }


        private void PlantsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.SelectedItemsChanged.Execute(Tuple.Create(e.AddedItems, e.RemovedItems));
        }




        public void ViewModelChanged(object vm)
        {
            try
            {
                this.ViewModel = (IGardenViewModel)vm;

            }
            catch { }
        }
    }
}