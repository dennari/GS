using Growthstories.WP8.Domain.Entities;
using Growthstories.WP8.Services;
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
using Growthstories.WP8.Domain.Interfaces;

namespace Growthstories.WP8.Domain.Entities.Plant
{

    public class Plant : AggregateBase<IDomainEvent>
    {

        private string _genus;

        private string _name;

        private PlantData _data;

        private IPlantDataService _dservice;

        private IPictureService _pservice;

        private string _picpath;

        private Stream _pic;

        //private ModelCollection<PlantAction> _actions;


        public Plant()
            : base()
        {
            //this._actions = new ModelCollection<PlantAction>(this);
        }

        public Plant(IEnumerable<IDomainEvent> events)
            : base(events)
        {

        }

        [Inject]
        public Plant(IPlantDataService dservice)
            : this()
        {
            this._dservice = dservice;

        }

        public Plant(string genus, IPlantDataService dservice)
            : this(dservice)
        {
            this._genus = genus;
        }

        public Garden Garden
        {
            get
            {
                return (Garden)Parent;
            }
            set
            {
                Parent = value;
            }
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
        [Column(Storage = "_genus")]
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
