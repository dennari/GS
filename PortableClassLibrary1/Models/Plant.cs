using Growthstories.PCL.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Growthstories.PCL.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Plant : INotifyPropertyChanged
    {
        private PlantData _data;
        
        private IPlantDataService _dservice;
        
        private string _genus;

        public string Name { get; set; }

        public IImage ProfilePicture { get; set; }

        public ISchedule Schedule { get; set; }

        public Plant(IPlantDataService dservice)
        {
            this._dservice = dservice;

        }

        public Plant(string genus, IPlantDataService dservice) : this(dservice)
        {
            this._genus = genus;
        }


        /// <summary>
        /// Gets or sets the plant info.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Genus
        {
            get
            {
                return this._genus;
            }
            set
            {
                this._genus = value;
                
                this.OnPropertyChanged();
            }
        }

        public async void load()
        {

            if (this._genus == null)
            {
                throw new MissingMemberException("Cannot load plant data until genus is set");
            }
            IList<PlantData> d = await this._dservice.LoadPlantDataAsync(this._genus);
            this._data = d[0];

        }

        /// <summary>
        /// Gets or sets the plant info.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public PlantData Info
        {
            get
            {
                return this._data;
            }
            set
            {
                this._data = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }


        }



    }


}
