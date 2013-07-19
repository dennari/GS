using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.UI.ViewModel
{
    public class ShowPlantView
    {
        public ShowPlantView(PlantCreated plant)
        {
            this.SelectedPlant = plant;
        }

        public PlantCreated SelectedPlant { get; private set; }
    }
}
