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
    public sealed class PlantState : AggregateState<PlantCreated>
    {

        public Guid UserId { get; private set; }

        public Guid WateringScheduleId { get; private set; }

        public Guid FertilizingScheduleId { get; private set; }

        public HashSet<string> Tags { get; private set; }

        public string Name { get; private set; }

        public string Species { get; private set; }

        public Photo Profilepicture { get; private set; }

        public IDictionary<Guid, PlantActionState> PlantActions { get; private set; }


        public PlantState()
        {
            this.Tags = new HashSet<string>();
        }

        public PlantState(PlantCreated @event)
            : this()
        {
            this.Apply(@event);
        }


        public override void Apply(PlantCreated @event)
        {
            base.Apply(@event);
            this.Name = @event.Name;
            this.Species = @event.Species;
            this.UserId = @event.UserId;
            this.Profilepicture = @event.Profilepicture;
            this.WateringScheduleId = @event.WateringScheduleId;
            this.FertilizingScheduleId = @event.FertilizingScheduleId;
            this.Tags = @event.Tags;

            this.PlantActions = new Dictionary<Guid, PlantActionState>();
        }

        public void Apply(PlantActionCreated @event)
        {

            if (@event.PlantId != this.Id)
            {
                throw new InvalidOperationException("Can't create plantaction under plant with incorrect id");
            }
            PlantActionState actionState = null;
            if (this.PlantActions.TryGetValue(@event.EntityId, out actionState))
            {
                throw new InvalidOperationException("PlantAction with this id already exists");
            }
            else
            {
                actionState = new PlantActionState(@event);
            }


            this.PlantActions[actionState.Id] = actionState;

        }

        public void Apply(PlantActionPropertySet @event)
        {
            if (@event.PlantId != this.Id)
            {
                throw new InvalidOperationException("Can't create plantaction under plant with incorrect id");
            }
            PlantActionState actionState = null;
            if (this.PlantActions.TryGetValue(@event.EntityId, out actionState))
            {
                actionState.Apply(@event);
            }
            else
            {
                throw new InvalidOperationException("PlantAction this id doesn't exist");
            }

        }


        //public void Apply(MarkedPlantPublic @event)
        //{
        //    Public = true;
        //}

        //public void Apply(MarkedPlantPrivate @event)
        //{
        //    Public = false;
        //}

        public void Apply(ProfilepictureSet @event)
        {
            this.Profilepicture = @event.Profilepicture;
        }

        public void Apply(WateringScheduleSet @event)
        {
            this.WateringScheduleId = @event.ScheduleId;
        }

        public void Apply(FertilizingScheduleSet @event)
        {
            this.FertilizingScheduleId = @event.ScheduleId;
        }
        public void Apply(TagsSet @event)
        {
            this.Tags = @event.Tags;
        }
        public void Apply(NameSet @event)
        {
            this.Name = @event.Name;
        }
        public void Apply(SpeciesSet @event)
        {
            this.Species = @event.Species;
        }

    }
}
