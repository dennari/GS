using GalaSoft.MvvmLight;
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
    public class AddPlantViewModel : ViewModelBase
    {
        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        private Garden _myGarden;
        private Plant _newPlant;
        private INavigationService _nav;

        public AddPlantViewModel([Named("My")] Garden myGarden, Plant newPlant, INavigationService nav)
        {
            this._myGarden = myGarden;
            this._newPlant = newPlant;
            this._nav = nav;
        }




        public Plant NewPlant
        {
            get
            {
                return this._newPlant;
            }
            private set
            {
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPlant"></param>
        public void save()
        {
            this._myGarden.Plants.Add(this._newPlant);
        }


    }
}
