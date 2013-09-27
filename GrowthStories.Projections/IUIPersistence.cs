using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.UI
{
    public interface IUIPersistence
    {

        void Purge();


        //void PersistAction(ActionBase a);
        void PersistAction(PlantActionState state);
        void PersistPlant(PlantState a);

        IEnumerable<PlantActionState> GetActions(Guid? PlantActionId = null, Guid? PlantId = null, Guid? UserId = null);
        //IEnumerable<ActionBase> UserActions(Guid UserId);
        //IEnumerable<PlantCreated> UserPlants(Guid UserId);







    }

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
    }
}
