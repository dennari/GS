using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Growthstories.Domain.Entities
{
    public sealed class PlantState : AggregateState<PlantCreated>
    {
        [JsonProperty]
        public bool Public { get; private set; }

        [JsonProperty]
        public Guid UserId { get; private set; }

        [JsonProperty]
        public Guid GardenId { get; private set; }
        [JsonProperty]
        public Guid? WateringScheduleId { get; private set; }
        [JsonProperty]
        public bool IsWateringScheduleEnabled { get; private set; }
        [JsonProperty]
        public Guid? FertilizingScheduleId { get; private set; }
        [JsonProperty]
        public bool IsFertilizingScheduleEnabled { get; private set; }
        [JsonProperty]
        public IList<string> Tags { get; private set; }
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty]
        public string Species { get; private set; }
        [JsonProperty]
        public Photo Profilepicture { get; private set; }
        [JsonProperty]
        public Guid? ProfilepictureActionId { get; private set; }




        public PlantState()
        {
            this.Tags = new List<string>();
            this.Public = false;
        }

        public PlantState(PlantCreated @event)
            : this()
        {
            this.Apply(@event);
        }


        public override void Apply(PlantCreated @event)
        {
            base.Apply(@event);

            if (@event.UserId == default(Guid))
            {
                throw DomainError.Named("empty_id", "UserId is required");
            }
            //if (@event.GardenId == default(Guid))
            //{
            //    throw DomainError.Named("empty_id", "GardenId is required");
            //}

            this.Name = @event.Name;
            this.UserId = @event.UserId;
            this.GardenId = @event.GardenId;


        }


        public void Apply(MarkedPlantPublic @event)
        {
            Public = true;
        }

        public void Apply(MarkedPlantPrivate @event)
        {
            Public = false;
        }

        public void Apply(ProfilepictureSet @event)
        {
            this.Profilepicture = @event.Profilepicture;
            this.ProfilepictureActionId = @event.PlantActionId;
        }

        public void Apply(ScheduleSet @event)
        {
            if (@event.Type == ScheduleType.WATERING)
                this.WateringScheduleId = @event.ScheduleId;
            else
                this.FertilizingScheduleId = @event.ScheduleId;
        }

        public void Apply(ScheduleToggled @event)
        {
            if (@event.Type == ScheduleType.WATERING)
                this.IsWateringScheduleEnabled = @event.IsEnabled;
            else
                this.IsFertilizingScheduleEnabled = @event.IsEnabled;
        }


        public void Apply(TagsSet @event)
        {
            this.Tags = @event.Tags.ToList();
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
