
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using System;
using System.Collections.Generic;


namespace Growthstories.UI.ViewModel
{



    public class AddPlantViewModel : DesignViewModelBase, IAddEditPlantViewModel
    {

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>


        public AddPlantViewModel()
        {


        }


        public IReactiveCommand ChooseProfilePictureCommand { get; protected set; }
        public IReactiveCommand ChooseWateringSchedule { get; protected set; }
        public IReactiveCommand AddTag { get; protected set; }
        public IReactiveCommand RemoveTag { get; protected set; }
        public IReactiveCommand ChooseFertilizingSchedule { get; protected set; }
        public IReactiveList<string> Tags { get; protected set; }
        public IScheduleViewModel WateringSchedule { get; set; }
        public IScheduleViewModel FertilizingSchedule { get; set; }
        public string ProfilePictureButtonText { get; set; }
        protected string FormatPlantTitle(string name, string species)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            return string.IsNullOrWhiteSpace(species) ? name.ToUpper() : string.Format("{0} ({1})", name.ToUpper(), species.ToUpper()); ;
        }

        protected string _Title = "new plant";
        public new string Title { get { return _Title; } set { _Title = value; } }
        public string Name { get; set; }
        public string Species { get; set; }
        public Guid Id { get; set; }
        public Photo Photo { get; set; }

    }



}