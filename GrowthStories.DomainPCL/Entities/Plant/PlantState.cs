using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Growthstories.Domain.Entities
{
    public class PlantState : AggregateState<PlantCreated>
    {

        public Guid UserId { get; private set; }

        protected string _Name;
        public string Name { get { return _Name; } protected set { Set(ref _Name, value); } }

        protected string _ProfilepicturePath;
        public string ProfilepicturePath { get { return _ProfilepicturePath; } protected set { Set(ref _ProfilepicturePath, value); } }



        public PlantState()
        {
        }

        public PlantState(PlantCreated @event)
        {
            this.Apply(@event);
        }


        public override void Apply(PlantCreated @event)
        {
            base.Apply(@event);
            this.Name = @event.Name;
            this.UserId = @event.UserId;
            this.ProfilepicturePath = @event.ProfilepicturePath;
        }

        public void Apply(MarkedPlantPublic @event)
        {
            Public = true;
        }

        public void Apply(MarkedPlantPrivate @event)
        {
            Public = false;
        }

        public void Apply(ProfilepicturePathChanged @event)
        {
            this.ProfilepicturePath = @event.ProfilepicturePath;
        }

    }
}
