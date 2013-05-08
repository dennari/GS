using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using Growthstories.Domain.Interfaces;

namespace Growthstories.Domain.Entities
{

    public class Garden : AggregateBase<IDomainEvent>
    {



        private User _owner;



        //public ModelCollection Notifications { get; set; }




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
                //this.OnPropertyChanged();
                //this.Notifications.
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

        }


        private IEnumerable<Plant> _plants;

        public IEnumerable<Plant> Plants
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

    public class GardenId : AbstractIdentity<Guid>
    {
        protected new string _tag = "garden";

        public GardenId(Guid id) : base(id) { }

    }
}
