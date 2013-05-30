using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using Growthstories.Domain.Messaging;
using Growthstories.Core;
using EventStore;
using CommonDomain;

namespace Growthstories.Domain.Entities
{

    public class Garden : AggregateBase<GardenState, GardenCreated>
    {

        public Garden()
        {

        }

        public void AddPlant(Guid plantId, string plantName)
        {
            RaiseEvent(new PlantAdded(this.Id, plantId, plantName));
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
                        RaiseEvent(new MarkedGardenPublic(Id));
                    }
                    else
                    {
                        RaiseEvent(new MarkedGardenPrivate(Id));
                    }
                }

            }
        }


    }


}
