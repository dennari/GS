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
using ReactiveUI;
using Growthstories.Domain.Messaging;
using Growthstories.Domain.Entities;
using Growthstories.UI.WindowsPhone.ViewModels;

namespace Growthstories.UI.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            //var scheduleType = ScheduleType.WATERING;
            //var vm = new ScheduleViewModel(scheduleType, 24 * 23 * 3600);
            //vm.OtherSchedules = new MockReactiveList<Tuple<IPlantViewModel, IScheduleViewModel>>() 
            //    {
            //        new Tuple<IPlantViewModel, IScheduleViewModel>(new PlantViewModel("AnotherPlant","Orvokki"), new ScheduleViewModel(scheduleType, 24*3*3600)),
            //        new Tuple<IPlantViewModel, IScheduleViewModel>(new PlantViewModel("Jaakko","Ruusu"), new ScheduleViewModel(scheduleType, 24*180*3600)),

            //    };
            //this.ScheduleView.ViewModel = vm;
            //this.ScheduleView.DataContext = vm;

        }


        //public static readonly DependencyProperty ViewModelProperty =
        // DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(MainPage), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));

        //public IRoutableViewModel ViewModel
        //{
        //    get
        //    {
        //        return (IRoutableViewModel)GetValue(ViewModelProperty);
        //    }
        //    set
        //    {
        //        SetValue(ViewModelProperty, value);
        //    }
        //}

        //object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (IPlantViewModel)value; } }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
            //var cvm = ViewModel.Router.GetCurrentViewModel() as Growthstories.UI.ViewModel.IControlsPageOrientation;
            //if (cvm != null)
            //{
            // ViewModel.PageOrientationChangedCommand.Execute((Growthstories.UI.ViewModel.PageOrientation)e.Orientation);
            //}

        }

        //public static List<IPlantActionViewModel> VMs = new List<IPlantActionViewModel>() {
        //    new PlantActionViewModel(),
        //    new PlantMeasureViewModel(),
        //    new ClientPlantPhotographViewModel(null,DateTimeOffset.Now),
        //    new PlantActionViewModel(PlantActionType.COMMENTED,DateTimeOffset.Now)
        //};

        //private int clicks;

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    this.PlantAction.ViewModel = VMs[clicks % 4];
        //    clicks++;
        //}

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}