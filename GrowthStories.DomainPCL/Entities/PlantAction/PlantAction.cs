

using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
namespace Growthstories.Domain.Entities
{

    public class PlantAction : AggregateBase<PlantActionState, PlantActionCreated>
    {


        public PlantAction()
        {

        }

    }

}
