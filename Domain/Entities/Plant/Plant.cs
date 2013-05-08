
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Growthstories.Domain.Interfaces;
using System.IO;
using System.Runtime.Serialization;

namespace Growthstories.Domain.Entities
{

    public class Plant : AggregateBase<IDomainEvent>
    {

        private string _genus;

        private string _name;

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


        //public Garden Garden
        //{
        //    get
        //    {
        //        return (Garden)Parent;
        //    }
        //    set
        //    {
        //        Parent = value;
        //    }
        //}

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
                //this.OnPropertyChanged();
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
                //this.OnPropertyChanged();
            }
        }



        // public ISchedule Schedule { get; set; }

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
                //this.OnPropertyChanged();
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
                //this.OnPropertyChanged();
            }
        }



        /// <summary>
        /// Gets or sets the plant info.
        /// </summary>
        /// <value>
        /// <typeparamref name="" />
        /// </value>
        //public PlantData Info
        //{
        //    get
        //    {
        //        return this._data;
        //    }
        //    set
        //    {
        //        this._data = value;
        //        this.OnPropertyChanged();
        //    }
        //}



    }

    public class PlantId : AbstractIdentity<Guid>
    {
        protected new string _tag = "plant";

        public PlantId(Guid id) : base(id) { }

    }


}
