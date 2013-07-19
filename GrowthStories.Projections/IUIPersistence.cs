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


        void PersistAction(ActionBase a);

        IEnumerable<ActionBase> PlantActions(Guid PlantId);
        IEnumerable<ActionBase> UserActions(Guid UserId);
        IEnumerable<PlantCreated> UserPlants(Guid UserId);


        void PersistPlant(PlantCreated a);



    }

    public class NullUIPersistence : IUIPersistence
    {



        public void Purge()
        {
        }

        public void PersistAction(ActionBase a)
        {
        }

        public IEnumerable<ActionBase> PlantActions(Guid PlantId)
        {
            return new ActionBase[0];
        }

        public IEnumerable<ActionBase> UserActions(Guid UserId)
        {
            return new ActionBase[0];
        }

        public IEnumerable<PlantCreated> UserPlants(Guid UserId)
        {
            return new PlantCreated[0];
        }

        public void PersistPlant(PlantCreated a)
        {
        }
    }
}
