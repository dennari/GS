using GalaSoft.MvvmLight;
using Growthstories.PCL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.PCL.ViewModel
{
    class PlantViewModel : ViewModelBase
    {
        private INavigationService _nav;

        public PlantViewModel(INavigationService nav)
        {
            this._nav = nav;
        }
    }
}
