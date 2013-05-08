using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using Growthstories.Domain.Interfaces;
using Growthstories.Domain.Messaging;

namespace Growthstories.Domain.Entities
{

    public class Garden : AggregateBase<IEvent<IIdentity>>
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

        public override void ThrowOnInvalidStateTransition(ICommand<IIdentity> c)
        {
            if (Version == 0)
            {
                if (c is CreateGarden)
                {
                    return;
                }
                throw DomainError.Named("premature", "Can't do anything to unexistent aggregate");
            }
            if (Version == -1)
            {
                throw DomainError.Named("zombie", "Can't do anything to deleted aggregate.");
            }
            if (c is CreateGarden)
                throw DomainError.Named("rebirth", "Can't create aggregate that already exists");

        }



        public Garden()
            : base()
        {
            //this._actions = new ModelCollection<PlantAction>(this);
        }

        public Garden(IEnumerable<IEvent<IIdentity>> events)
            : base(events)
        {

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

        public void Create(GardenId plantId)
        {
            Apply(new GardenCreated(plantId));
        }

        public void When(GardenCreated e)
        {
            Id = e.EntityId;
        }




    }

    public class GardenId : AbstractIdentity<Guid>
    {
        protected new string _tag = "garden";

        public GardenId(Guid id) : base(id) { }

    }
}
