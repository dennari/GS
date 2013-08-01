using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Domain.Entities
{
    public class PlantState : AggregateState<PlantCreated>
    {
        private readonly IList<string> _Comments = new List<string>();
        public IList<string> Comments { get { return _Comments; } }

        private readonly IList<string> _Photos = new List<string>();
        public string Name { get; private set; }
        public IList<string> Photos { get { return _Photos; } }

        public Guid UserId { get; private set; }

        public string ProfilepicturePath { get; private set; }

        public event EventHandler ProfilepicturePathChanged;

        public PlantState()
        {
        }

        public PlantState(PlantCreated @event)
        {
            this.Apply(@event);
        }
        //public PlantState(Guid id, int version, bool Public)
        //    : base(id, version, Public)
        //{
        //}

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
            if (this.ProfilepicturePathChanged != null)
                this.ProfilepicturePathChanged(this, new EventArgs());
        }

    }
}
