using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Projections
{
    public class PlantProjection : ProjectionBase, IEventHandler<PlantCreated>
    {

        private IList<string> _PlantNames = new List<string>();
        public IList<string> PlantNames { get { return _PlantNames; } }

        public void Handle(PlantCreated @event)
        {
            this.PlantNames.Add(@event.Name);
        }

    }
}
