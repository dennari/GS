using Growthstories.Domain.Entities;
using Growthstories.Domain.Interfaces;
using System;


namespace Growthstories.Domain.Messaging
{

    public class CreateUser : ICommand<UserId>
    {
        public UserId EntityId { get; private set; }

        CreateUser() { }
        public CreateUser(UserId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Create user {0}.", EntityId);
        }

    }

    public class UserCreated : IEvent<UserId>
    {
        public UserId EntityId { get; private set; }

        UserCreated() { }
        public UserCreated(UserId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Created user {0}", EntityId);
        }

    }

    public class DeleteUser : ICommand<UserId>
    {
        public UserId EntityId { get; private set; }

        DeleteUser() { }
        public DeleteUser(UserId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Delete user {0}.", EntityId);
        }

    }

    public class CreatePlant : ICommand<PlantId>
    {
        public PlantId EntityId { get; private set; }

        CreatePlant() { }
        public CreatePlant(PlantId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Create plant {0}.", EntityId);
        }

    }

    public class PlantCreated : IEvent<PlantId>
    {
        public PlantId EntityId { get; private set; }

        PlantCreated() { }
        public PlantCreated(PlantId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Created plant {0}", EntityId);
        }

    }

    public class DeletePlant : ICommand<PlantId>
    {
        public PlantId EntityId { get; private set; }

        DeletePlant() { }
        public DeletePlant(PlantId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Delete plant {0}.", EntityId);
        }

    }

    public class CreateGarden : ICommand<GardenId>
    {
        public GardenId EntityId { get; private set; }

        CreateGarden() { }
        public CreateGarden(GardenId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Create plant {0}.", EntityId);
        }

    }

    public class GardenCreated : IEvent<GardenId>
    {
        public GardenId EntityId { get; private set; }

        GardenCreated() { }
        public GardenCreated(GardenId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Created plant {0}", EntityId);
        }

    }

    public class DeleteGarden : ICommand<GardenId>
    {
        public GardenId EntityId { get; private set; }

        DeleteGarden() { }
        public DeleteGarden(GardenId id)
        {
            EntityId = id;
        }

        public override string ToString()
        {
            return string.Format(@"Delete plant {0}.", EntityId);
        }

    }

}

