using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Growthstories.PCL.Models
{
    public class PhotoAction : PlantAction
    {

        private Stream _photo;

        public PhotoAction(Plant plant, Stream photo)
            : base(plant)
        {
            _photo = photo;
        }

    }
}
