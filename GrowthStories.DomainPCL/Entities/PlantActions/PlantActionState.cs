
using CommonDomain;
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
        NOTYPE,
        WATERED,
        FERTILIZED,
        PHOTOGRAPHED,
        MEASURED,
        COMMENTED,
        FBCOMMENTED,
        MISTED,
        BLOOMED,
        SPROUTED,
        PRUNED,
        DECEASED,
        POLLINATED,
        TRANSFERRED,
        HARVESTED
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
        public double? Value { get; private set; }

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


            if (@event.UserId == default(Guid))
            {
                throw DomainError.Named("empty_id", "UserId is required");
            }
            if (@event.PlantId == default(Guid))
            {
                throw DomainError.Named("empty_id", "PlantId is required");
            }
            if (@event.Type == PlantActionType.NOTYPE)
            {
                throw DomainError.Named("empty_id", "PlantActionType is required");
            }
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
            //if (@event.Type != this.Type)
            //    throw new InvalidOperationException("Can't set properties of incompatible PlantActionTypes");

            if (@event.Note != null)
                this.Note = @event.Note;

            if (Type == PlantActionType.MEASURED)
                this.Value = @event.Value;
            if (Type == PlantActionType.PHOTOGRAPHED)
                this.Photo = @event.Photo;

        }

        public void Apply(BlobKeySet @event)
        {
            if (this.Type != PlantActionType.PHOTOGRAPHED)
                throw DomainError.Named("invalid_type", "Can't set BlobKey for an action that isn't a photograph.");
            if (this.Photo == null)
                throw DomainError.Named("photo_not_set", "Can't set BlobKey without a photo.");

            this.Photo.BlobKey = @event.BlobKey;
            if (@event.Pmd != null)
            {
                this.Photo.RemoteUri = @event.Pmd.RemoteUri;
                this.Photo.Width = @event.Pmd.Width;
                this.Photo.Height = @event.Pmd.Height;

            }
        }


    }
}
