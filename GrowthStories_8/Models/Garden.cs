using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Growthstories.WP8.Models;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using System;

namespace Growthstories.WP8.Models
{

    [Table]
    public class Garden : ModelBase
    {



        private User _owner;



        public ObservableCollection<Notification> Notifications { get; set; }




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
            }
        }

        public void ActOnSelected()
        {
        }

        public void ActOnAll()
        {
        }



        public Garden(User owner)
        {
            _owner = owner;
            Notifications = new ObservableCollection<Notification>();
            this._plants = new EntitySet<Plant>(
                (Plant p) =>
                {
                    OnPropertyChanging();
                    p.Garden = this;
                },
                (Plant p) =>
                {
                    OnPropertyChanging();
                    p.Garden = null;
                }
                );

        }


        // Entity reference, to identify the ToDoCategory "storage" table
        private EntitySet<Plant> _plants;



        // Association, to describe the relationship between this key and that "storage" table
        [Association(Storage = "_plants", OtherKey = "_gardenId")]
        new public EntitySet<Plant> Plants
        {
            get
            {
                return _plants;
            }
            private set
            {
                OnPropertyChanging();
                _plants.Assign(value);
                OnPropertyChanged();
            }
        }


    }
}
public class Notification
{
    public string Msg { get; set; }
    public string Icon { get; set; }
}
