using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI
{
    public class PlantProjection : EventHandlerBase, IEventHandler<PlantCreated>
    {



        private readonly IUIPersistence Persistence;

        public PlantProjection(IUIPersistence persistence)
        {
            if (persistence == null)
                throw new ArgumentNullException("uipersistence");
            this.Persistence = persistence;
            //PlantNames.Add("Jore");
        }

        public void Handle(PlantCreated @event)
        {
            this.Persistence.PersistPlant(@event);
        }

        public IEnumerable<PlantCreated> LoadWithUserId(Guid UserId)
        {
            return Persistence.UserPlants(UserId);
        }

    }
}
