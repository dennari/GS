using CommonDomain;
using EventStore;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Entities
{

    public enum PlantActionType
    {
        WATERED,
        FERTILIZED,
        PHOTOGRAPHED,
        MEASURED,
        COMMENTED
    }


    public class PlantActionState : AggregateState<PlantActionCreated>
    {

        [JsonProperty]
        public PlantActionType Type { get; private set; }

        [JsonProperty]
        public Guid UserId { get; private set; }

        [JsonProperty]
        public Guid PlantId { get; private set; }

        [JsonProperty]
        public string Note { get; private set; }

        [JsonProperty]
        public MeasurementType MeasurementType { get; private set; }

        [JsonProperty]
        public double Value { get; private set; }

        [JsonProperty]
        public Photo Photo { get; private set; }

        public PlantActionState()
            : base()
        {
        }

        public PlantActionState(PlantActionCreated e)
            : this()
        {
            this.Apply(e);
        }


        public override void Apply(PlantActionCreated @event)
        {
            base.Apply(@event);
            this.Type = @event.Type;
            this.UserId = @event.UserId;
            this.PlantId = @event.PlantId;
            this.Note = @event.Note;
            this.MeasurementType = @event.MeasurementType;
            this.Value = @event.Value;
            this.Photo = @event.Photo;

        }

        public void Apply(PlantActionPropertySet @event)
        {
            if (@event.Type != this.Type)
                throw new InvalidOperationException("Can't set properties of incompatible PlantActionTypes");

            if (@event.Note != null)
                this.Note = @event.Note;

            if (@event.Type == PlantActionType.MEASURED)
                this.Value = @event.Value;
            if (@event.Type == PlantActionType.PHOTOGRAPHED)
                this.Photo = @event.Photo;

        }



    }
}
