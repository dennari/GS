
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using Growthstories.Domain.Messaging;
using CommonDomain;
using CommonDomain.Core;
using Growthstories.Core;

namespace Growthstories.Domain.Entities
{

    public class Plant : AggregateBase<PlantState, PlantCreated>
    {


        public Plant()
        {

        }

        public bool Public
        {
            get
            {
                return State.Public ?? false;
            }
            set
            {
                if (!State.Public.HasValue || State.Public.Value != value)
                {
                    if (value == true)
                    {
                        RaiseEvent(new MarkedPlantPublic(Id));
                    }
                    else
                    {
                        RaiseEvent(new MarkedPlantPrivate(Id));
                    }
                }

            }
        }


        public new void Create(Guid Id)
        {
            throw new NotSupportedException();
        }

        public void Create(AddPlant c)
        {
            if (State == null)
            {
                ApplyState(null);
            }
            RaiseEvent(new PlantCreated(c.PlantId, c.PlantName));
        }

        public void Create(CreatePlant c)
        {
            if (State == null)
            {
                ApplyState(null);
            }
            RaiseEvent(new PlantCreated(c.EntityId, c.Name));
        }
    }

}
