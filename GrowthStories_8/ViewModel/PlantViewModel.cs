using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Growthstories.PCL.Helpers;
using Growthstories.WP8.Domain.Entities;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Growthstories.WP8.ViewModel
{
    public class PlantViewModel : ViewModelBase
    {
        private INavigationService _nav;

        private Plant _plant;

        private PlantAction _selectedAction;

        private RelayCommand<PlantAction> _navigateToSelectedAction;
        private const string SelectedActionPageUrl = "ActionPage";

        public PlantViewModel()
            : base()
        {

        }

        [Inject]
        public PlantViewModel(INavigationService nav)
        {
            this._nav = nav;

        }

        public PlantAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                Set("SelectedAction", ref _selectedAction, value);
            }
        }

        public Plant CurrentPlant
        {
            get
            {
                return _plant;
            }
            set
            {
                if (value != null)
                {
                    Set("CurrentPlant", ref _plant, value);
                }
            }
        }

        /// <summary>
        /// Gets the NavigateToArticleCommand.
        /// </summary>
        public RelayCommand<PlantAction> NavigateToAction
        {
            get
            {
                if (_navigateToSelectedAction == null)
                {
                    _navigateToSelectedAction = new RelayCommand<PlantAction>((action) =>
                    {

                        SelectedAction = action;
                        _nav.NavigateTo(new Uri(string.Format("/View/{0}.xaml", SelectedActionPageUrl), UriKind.Relative));
                    });
                }
                return _navigateToSelectedAction;

            }
        }

        public void SelectedPlantChanged(object sender, PropertyChangedEventArgs e)
        {
            GardenViewModel s = sender as GardenViewModel;
            if (sender != null && e.PropertyName == GardenViewModel.PlantPropertyName)
            {
                CurrentPlant = s.SelectedPlant;
            }
        }

    }
}
