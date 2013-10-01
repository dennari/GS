using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;
using System.Collections.Generic;


namespace Growthstories.Domain.Messaging
{

    public abstract class PlantCommand : EntityCommand<Plant>
    {
        public PlantCommand() { }
        public PlantCommand(Guid EntityId) : base(EntityId) { }
    }

    #region Plant
    public class CreatePlant : PlantCommand, ICreateCommand
    {

        public string Name { get; private set; }

        public string Species { get; set; }

        public Photo Profilepicture { get; set; }

        public Guid WateringScheduleId { get; set; }

        public Guid FertilizingScheduleId { get; set; }

        public HashSet<string> Tags { get; set; }

        public Guid UserId { get; private set; }

        public CreatePlant() { }
        public CreatePlant(Guid id, string name, Guid userId)
            : base(id)
        {
            Name = name;
            this.UserId = userId;
            this.AncestorId = userId;
            this.ParentAncestorId = userId;
            this.StreamEntityId = id;
            this.StreamAncestorId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Create plant {0}.", EntityId);
        }



    }

    public class SetProfilepicture : PlantCommand
    {

        public Photo Profilepicture { get; private set; }

        protected SetProfilepicture() { }
        public SetProfilepicture(Guid entityId, Photo profilepicture)
            : base(entityId)
        {
            this.Profilepicture = profilepicture;
        }

        public override string ToString()
        {
            return string.Format(@"Change ProfilepicturePath to {0}", Profilepicture);
        }

    }

    public class MarkPlantPublic : PlantCommand
    {
        protected MarkPlantPublic() { }
        public MarkPlantPublic(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Mark plant {0} public", EntityId);
        }

    }

    public class MarkPlantPrivate : PlantCommand
    {
        protected MarkPlantPrivate() { }
        public MarkPlantPrivate(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Mark plant {0} private", EntityId);
        }

    }

    public class DeletePlant : PlantCommand
    {

        public DeletePlant() { }
        public DeletePlant(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete plant {0}.", EntityId);
        }

    }


    public class SetName : PlantCommand
    {
        public string Name { get; private set; }

        public SetName(Guid plantId, string name)
            : base(plantId)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return string.Format(@"Set name to {1} for plant {0}.", EntityId, Name);
        }

    }

    public class SetSpecies : PlantCommand
    {
        public string Species { get; private set; }

        public SetSpecies(Guid plantId, string species)
            : base(plantId)
        {
            this.Species = species;
        }

        public override string ToString()
        {
            return string.Format(@"Set species to {1} for plant {0}.", EntityId, Species);
        }

    }


    public class SetWateringSchedule : PlantCommand
    {
        public Guid ScheduleId { get; set; }

        public SetWateringSchedule() { }
        public SetWateringSchedule(Guid id, Guid scheduleId)
            : base(id)
        {
            this.ScheduleId = scheduleId;
        }

        public override string ToString()
        {
            return string.Format(@"Set watering schedule {1} to plant {0}.", EntityId, ScheduleId);
        }

    }

    public class SetFertilizingSchedule : PlantCommand
    {
        public Guid ScheduleId { get; set; }

        public SetFertilizingSchedule() { }
        public SetFertilizingSchedule(Guid plantId, Guid scheduleId)
            : base(plantId)
        {
            this.ScheduleId = scheduleId;
        }

        public override string ToString()
        {
            return string.Format(@"Set fertilizing schedule {1} to plant {0}.", EntityId, ScheduleId);
        }

    }

    public class SetTags : PlantCommand
    {
        public HashSet<string> Tags { get; private set; }

        public SetTags() { }
        public SetTags(Guid plantId, HashSet<string> tags)
            : base(plantId)
        {
            this.Tags = tags;
        }

        public override string ToString()
        {
            return string.Format(@"Set tags for plant {0}.", EntityId);
        }

    }


    #endregion


}

