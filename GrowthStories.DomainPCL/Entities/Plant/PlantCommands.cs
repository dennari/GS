using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;
using System.Collections.Generic;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{

    public abstract class PlantCommand : AggregateCommand<Plant>
    {
        public PlantCommand() { }
        public PlantCommand(Guid EntityId) : base(EntityId) { }
    }

    #region Plant
    public class CreatePlant : PlantCommand, ICreateMessage
    {

        public string Name { get; private set; }
        public Guid GardenId { get; private set; }
        public Guid UserId { get; private set; }


        //public CreatePlant() { }
        public CreatePlant(Guid id, string name, Guid gardenId, Guid UserId)
            : base(id)
        {
            Name = name;
            this.GardenId = gardenId;
            this.UserId = UserId;

        }

        public override string ToString()
        {
            return string.Format(@"Create plant {0}, {1}.", Name, AggregateId);
        }



    }

    public class SetProfilepicture : PlantCommand
    {

        //public Photo Profilepicture { get; private set; }
        public Guid PlantActionId { get; private set; }


        protected SetProfilepicture() { }
        public SetProfilepicture(Guid entityId, Guid plantActionId)
            : base(entityId)
        {
            //this.Profilepicture = profilepicture;
            this.PlantActionId = plantActionId;
        }

        public override string ToString()
        {
            return string.Format(@"Change Profilepicture to {0}", PlantActionId);
        }

    }


    public class MarkPlantPublic : PlantCommand
    {
        protected MarkPlantPublic() { }
        public MarkPlantPublic(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Mark plant {0} public", AggregateId);
        }

    }

    public class MarkPlantPrivate : PlantCommand
    {
        protected MarkPlantPrivate() { }
        public MarkPlantPrivate(Guid entityId) : base(entityId) { }

        public override string ToString()
        {
            return string.Format(@"Mark plant {0} private", AggregateId);
        }

    }

    public class ToggleSchedule : PlantCommand
    {
        public readonly bool IsEnabled;
        public readonly ScheduleType Type;

        protected ToggleSchedule() { }
        public ToggleSchedule(Guid plantId, bool isEnabled, ScheduleType type)
            : base(plantId)
        {
            this.IsEnabled = isEnabled;
            this.Type = type;
        }

        public override string ToString()
        {
            return string.Format(@"Set {0} schedule to {1}", Type, IsEnabled);
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
            return string.Format(@"Set name to {1} for plant {0}.", AggregateId, Name);
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
            return string.Format(@"Set species to {1} for plant {0}.", AggregateId, Species);
        }

    }


    public class SetWateringSchedule : PlantCommand
    {
        public Guid? ScheduleId { get; set; }

        //public SetWateringSchedule() { }
        public SetWateringSchedule(Guid plantId, Guid? scheduleId)
            : base(plantId)
        {
            this.ScheduleId = scheduleId;
        }

        public override string ToString()
        {
            return string.Format(@"Set watering schedule {1} to plant {0}.", AggregateId, ScheduleId);
        }

    }

    public class SetFertilizingSchedule : PlantCommand
    {
        public Guid? ScheduleId { get; set; }

        //public SetFertilizingSchedule() { }
        public SetFertilizingSchedule(Guid plantId, Guid? scheduleId)
            : base(plantId)
        {
            this.ScheduleId = scheduleId;
        }

        public override string ToString()
        {
            return string.Format(@"Set fertilizing schedule {1} to plant {0}.", AggregateId, ScheduleId);
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
            return string.Format(@"Set tags for plant {0}.", AggregateId);
        }
    }


    public class SetLocation : PlantCommand
    {

        public GSLocation Location;


        public SetLocation() { }

        public SetLocation(Guid plantId, GSLocation location)
            : base(plantId)
        {
            this.Location = location;
        }

        public override string ToString()
        {
            return string.Format(@"Set location for plant {0}", AggregateId);
        }

    }


    #endregion


}

