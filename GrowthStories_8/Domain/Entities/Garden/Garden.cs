using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Growthstories.WP8.Domain.Entities;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using System;
using Ninject;

namespace Growthstories.WP8.Domain.Entities
{

    public class Garden : ModelBase
    {



        private User _owner;



        public ModelCollection Notifications { get; set; }




        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public User Owner
        {
            get
            {
                return this._owner;
            }
            set
            {
                this._owner = value;
                this.OnPropertyChanged();
                this.Notifications.
            }
        }

        public void ActOnSelected()
        {
        }

        public void ActOnAll()
        {
        }


        public Garden()
        {
            _plants = new ModelCollection<Plant>(this);
            Notifications = new ModelCollection(this);
        }

        [Inject]
        public Garden(User owner)
            : this()
        {
            _owner = owner;

        }

        private List<Plant> _plantsDb;

        [Association(OtherKey = "ParentId")]
        public List<Plant> PlantsDb
        {
            get
            {
                return _plants.ToList();
            }
            private set
            {
                value.ForEach(p => p.Parent = this);
                _plants = new ModelCollection<Plant>(this, value);
            }
        }


        private ModelCollection<Plant> _plants;

        public ModelCollection<Plant> Plants
        {
            get
            {
                return _plants;
            }
            private set
            {
                //OnPropertyChanging();
                _plants = value;
                //OnPropertyChanged();
            }
        }


    }
}
public class Notification : ModelBase
{
    public string Msg { get; set; }
    public string Icon { get; set; }
}
