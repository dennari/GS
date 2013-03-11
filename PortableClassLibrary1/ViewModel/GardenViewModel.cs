using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Growthstories.PCL.Helpers;
using Growthstories.PCL.Models;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Growthstories.PCL.ViewModel
{
    public class GardenViewModel : ViewModelBase
    {
        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        private Garden _myGarden;

        private INavigationService _nav;

        private RelayCommand<Plant> _navigateToPlant;

        /// <summary>
        /// The <see cref="SelectedPlant" /> property's name.
        /// </summary>
        public const string PlantPropertyName = "SelectedPlant";

        private Plant _selectedPlant;

        /// <summary>
        /// Sets and gets the Plant property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Plant SelectedPlant
        {
            get
            {
                return _selectedPlant;
            }
            set
            {
                Set(PlantPropertyName, ref _selectedPlant, value);
            }
        }

        private const string PlantPageUrl = "PlantPage";


        public GardenViewModel([Named("My")] Garden myGarden, INavigationService nav)
        {
            this._myGarden = myGarden;
            this._nav = nav;
        }

        /// <summary>
        /// Gets the NavigateToArticleCommand.
        /// </summary>
        public RelayCommand<Plant> NavigateToPlant
        {
            get
            {
                if (_navigateToPlant == null)
                {
                    _navigateToPlant = new RelayCommand<Plant>((plant) =>
                    {

                        SelectedPlant = plant;
                        _nav.NavigateTo(new Uri(string.Format("/View/{0}.xaml", PlantPageUrl), UriKind.Relative));
                    });
                }
                return _navigateToPlant;

            }
        }

        /// <summary>
        /// Gets or sets the garden
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public Garden MyGarden
        {
            get
            {
                return this._myGarden;
            }
            set
            {
                this._myGarden = value;
                this.RaisePropertyChanged(() => this.MyGarden);
            }
        }


    }
}
