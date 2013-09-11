
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{


    public class PlantStateViewModel : GSViewModelBase
    {
        private readonly PlantState State;

        public Guid Id { get { return State.Id; } }
        public Guid UserId { get { return State.UserId; } }
        public string Name { get { return State.Name; } }
        public string ProfilepicturePath { get { return State.ProfilepicturePath; } }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PlantStateViewModel(PlantState state, IMessageBus bus)
            : base(bus)
        {
            this.State = state;
            //this.State.ProfilepicturePathChanged += State_ProfilepicturePathChanged;
        }

        void State_ProfilepicturePathChanged(object sender, EventArgs e)
        {
            //Sthis.RaisePropertyChanged("ProfilepicturePath");
        }


    }
}