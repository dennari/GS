using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Growthstories.PCL.Models
{
    public abstract class PlantAction : INotifyPropertyChanged
    {
        private Plant _plant;

        private DateTimeOffset? _createdAt;

        private DateTimeOffset? _modifiedAt;

        private String _note;

        public event PropertyChangedEventHandler PropertyChanged;

        public PlantAction()
        {

        }

        public PlantAction(Plant plant)
        {
            this._plant = plant;
            this._createdAt = DateTimeOffset.Now;
            this._modifiedAt = this._createdAt;



        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public Plant Plant
        {
            get
            {
                return this._plant;
            }
            private set
            {

            }
        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public String Note
        {
            get
            {
                return this._note;
            }
            set
            {
                this._note = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }
}
