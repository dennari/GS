using Growthstories.PCL.Models;
using Growthstories.PCL.Services;
using Ninject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.WP8.Models
{

    [Table]
    public class Plant : ModelBase
    {

        private string _genus;

        private string _name;

        private PlantData _data;

        private IPlantDataService _dservice;

        private IPictureService _pservice;

        private string _picpath;

        private Stream _pic;

        private ObservableCollection<PlantAction> _actions;

        // Internal column for the associated Garden ID value
        [Column]
        internal int _gardenId;

        // Entity reference, to identify the ToDoCategory "storage" table
        private EntityRef<Garden> _garden;

        [Inject]
        public Plant(IPlantDataService dservice)
        {
            this._dservice = dservice;
            this._actions = new ObservableCollection<PlantAction>();
        }

        public Plant(string genus, IPlantDataService dservice)
            : this(dservice)
        {
            this._genus = genus;
        }






        // Association, to describe the relationship between this key and that "storage" table
        [Association(Storage = "_garden", ThisKey = "_gardenId", OtherKey = "Id", IsForeignKey = true)]
        public Garden Garden
        {
            get
            {
                return _garden.Entity;
            }
            set
            {
                OnPropertyChanging();
                _garden.Entity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the plant name.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        [Column]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                OnPropertyChanging();
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
        [Column]
        public string Genus
        {
            get
            {
                return this._genus;
            }
            set
            {
                OnPropertyChanging();
                this._genus = value;
                this.OnPropertyChanged();
            }
        }




        public ObservableCollection<PlantAction> Actions
        {
            get
            {
                return this._actions;
            }
            private set { }
        }


        public ISchedule Schedule { get; set; }

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
            //IList<PlantData> d = await this._dservice.LoadPlantDataAsync(this._genus);
            //this._data = d[0];

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



    }


}
