using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.WP8.Domain.Entities
{
    public class PhotoAction : PlantAction
    {


        public PhotoAction()
        {

        }


        public PhotoAction(Plant plant, Uri photoUri)
            : base(plant)
        {
            this._photoUri = photoUri;
        }

        private Uri _photoUri;

        public Uri PhotoUri
        {
            get { return _photoUri; }
            set
            {
                _photoUri = value;
                OnPropertyChanged();
            }
        }

    }
}
