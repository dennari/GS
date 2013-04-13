using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.WP8.Models.Commands
{
    class CommandFactory
    {
        public NewPlantCommand NewPlant(string name, string genus)
        {
            var cmd = new NewPlantCommand()
            {
                name = name,
                genus = genus
            };
            return cmd;
        }
    }
}
