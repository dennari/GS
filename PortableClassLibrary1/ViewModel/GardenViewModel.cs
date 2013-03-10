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

        private const string PlantPageUrl = "/PlantPage.xaml?item={0}";


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
                return _navigateToPlant
                    ?? (_navigateToPlant = new RelayCommand<Plant>(
                        (plant) =>
                        {
#if NETFX_CORE
                            _navigationService.NavigateTo(typeof (DetailsPage), article);
#else
                           
                            _nav.NavigateTo(new Uri(string.Format(PlantPageUrl, url), UriKind.Relative));
#endif
                        }));
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
