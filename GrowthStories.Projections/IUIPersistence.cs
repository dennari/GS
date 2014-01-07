using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.UI
{


    public class NullUIPersistence : IUIPersistence
    {



        public void Purge()
        {

        }


        public IEnumerable<PlantCreated> UserPlants(Guid UserId)
        {
            return new PlantCreated[0];
        }

        public void PersistPlant(PlantCreated a)
        {
        }


        public PlantActionState GetLatestWatering(Guid? PlantId)
        {
            return null;
        }


        public void PersistAction(Domain.Entities.PlantActionState state)
        {
        }


        public void PersistPlant(PlantState a)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PlantActionState> GetActions(Guid? PlantActionId = null, Guid? PlantId = null, Guid? UserId = null)
        {
            return new PlantActionState[0];
        }

        public IEnumerable<PlantActionState> GetPhotoActions(Guid? PlantId = null)
        {
            return new PlantActionState[0];
        }

        public void Save(Core.IGSAggregate aggregate)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<PlantState> GetPlants(Guid? PlantId = null, Guid? GardenId = null, Guid? UserId = null)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<UserState> GetUsers(Guid? UserId = null)
        {
            throw new NotImplementedException();
        }


        public void SaveCollaborator(Guid collaboratorId, bool status)
        {
            throw new NotImplementedException();
        }


        IEnumerable<Tuple<PlantState, ScheduleState, ScheduleState>> IUIPersistence.GetPlants(Guid? PlantId = null, Guid? GardenId = null, Guid? UserId = null)
        {
            throw new NotImplementedException();
        }


        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
