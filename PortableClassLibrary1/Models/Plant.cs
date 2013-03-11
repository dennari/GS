using Growthstories.PCL.Services;
using Ninject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.PCL.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Plant : INotifyPropertyChanged
    {
        private PlantData _data;

        private IPlantDataService _dservice;

        private IPictureService _pservice;

        private string _genus;

        private string _name;

        private string _picpath;

        private Stream _pic;

        public ObservableCollection<PlantAction> Actions { get; private set; }


        public ISchedule Schedule { get; set; }

        public Plant()
        {

        }

        [Inject]
        public Plant(IPlantDataService dservice)
        {
            this._dservice = dservice;
            this.Actions = new ObservableCollection<PlantAction>();
        }

        public Plant(string genus, IPlantDataService dservice)
            : this(dservice)
        {
            this._genus = genus;
        }


        /// <summary>
        /// Gets or sets the plant name.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the plant genus.
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

        /// <summary>
        /// Gets or sets the plant genus.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string ProfilePicturePath
        {
            get
            {
                return this._picpath;
            }
            set
            {
                this._picpath = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the plant genus.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public Stream ProfilePicture
        {
            get
            {
                return this._pic;
            }
            set
            {
                this._pic = value;
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
        /// <typeparamref name="" />
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
