using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Growthstories.PCL.Models
{
    public class Garden : INotifyPropertyChanged
    {
        private User _owner;


        public ObservableCollection<Plant> Plants { get; private set; }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public Garden()
        {

        }

        public Garden(User owner)
        {
            _owner = owner;
            Plants = new ObservableCollection<Plant>();
            Plants.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Plants_CollectionChanged);
        }

        void Plants_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public User Owner
        {
            get
            {
                return this._owner;
            }
            set
            {
                this._owner = value;
                this.OnPropertyChanged();
            }
        }

        public void ActOnSelected()
        {
        }

        public void ActOnAll()
        {
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
