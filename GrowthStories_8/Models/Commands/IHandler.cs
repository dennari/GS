using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.WP8.Models.Commands
{
    public interface IHandler
    {

        void NewPlant(PlantDTO plant);
    }
}
