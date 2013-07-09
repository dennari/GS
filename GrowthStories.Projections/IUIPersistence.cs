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


        void PersistPlant(PlantCreated a);

    }
}
