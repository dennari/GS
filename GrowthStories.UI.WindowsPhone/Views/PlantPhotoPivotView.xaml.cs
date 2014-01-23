using System;
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
using Telerik.Windows.Controls;


namespace Growthstories.UI.WindowsPhone
{

    public class PlantPhotoPivotViewBase : GSView<IPhotoListViewModel>
    {

    }


    public partial class PlantPhotoPivotView : PlantPhotoPivotViewBase
    {

        public PlantPhotoPivotView()
        {
            InitializeComponent();

            if (Height != Double.NaN)
            {
                Height = Double.NaN;
            }
        }


        protected override void OnViewModelChanged(IPhotoListViewModel vm)
        {
            //RadSlideView view = TheSlideView;

            //view.SelectedItem = vm.Selected;
            //view.
            //vm.WhenAnyValue(x => vm.Selected).Subscribe(x =>
            //{
            //    view.SelectedItem = x;
            //});
        }


        private void TheSlideView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Log().Info("setting selected  to #{0}, {1}", ViewModel.Selected.ActionIndex, ViewModel.Selected.PlantActionId);
            TheSlideView.SelectedItem = ViewModel.Selected;
            //TheSlideView.StartSlideShow();
            //TheSlideView.StopSlideShow();
            //TheSlideView.UpdateLayout();
        }


    }
}