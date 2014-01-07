using GalaSoft.MvvmLight;
using Growthstories.PCL.Helpers;
using Growthstories.WP8.Domain.Entities;
using Growthstories.WP8.Services;

using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Growthstories.WP8.ViewModel
{
    public class AddPlantViewModel : ViewModelBase
    {
        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        private Stream _photo;



        public INavigationService Nav { get; private set; }

        public IDataService Data { get; private set; }

        public Stream ProfilePhoto
        {
            get
            {
                return _photo;
            }
            set
            {
                _photo = value;
                RaisePropertyChanged("ProfilePhoto");
            }

        }

        public AddPlantViewModel(IDataService data, INavigationService nav)
        {
            Data = data;
            Nav = nav;
        }

        public void save(string name, string genus)
        {
            if (!validName(name) || !validGenus(genus))
            {
                /// <todo>
                /// 
                /// </todo>
                return;
            }
            //Plant p = Data.getNewPlant();
            //p.Name = name;
            //p.Genus = genus;
            //Data.AddCommand(new NewPlantCommand());

        }

        private bool validGenus(string genus)
        {
            return genus.Length > 0;
        }

        private bool validName(string name)
        {
            return validGenus(name);
        }
    }
}
