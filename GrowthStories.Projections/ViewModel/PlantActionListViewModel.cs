using Growthstories.Domain.Entities;
using Growthstories.Domain;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using Growthstories.Core;
//using EventStore.Logging;
using CommonDomain;
using System.Collections;

namespace Growthstories.UI.ViewModel
{

    public sealed class PlantActionItem
    {
        public IconType Icon { get; set; }
        public string Title { get; set; }
        public IReactiveCommand Command { get; set; }

    }

    public sealed class PlantActionListViewModel : RoutableViewModel, IPlantActionListViewModel
    {
        private readonly IPlantViewModel Plant;



        public IReadOnlyReactiveList<PlantActionItem> PlantActions { get; private set; }

        public PlantActionListViewModel(IPlantViewModel plant, IGSAppViewModel app)
            : base(app)
        {
            this.Plant = plant;
            var plantActions = new ReactiveList<PlantActionItem>();
            foreach (var o in PlantActionViewModel.ActionTypeToLabel)
            {
                plantActions.Add(new PlantActionItem()
                {
                    Title = o.Value,
                    Icon = PlantActionViewModel.ActionTypeToIcon[o.Key],
                    Command = Plant.AddActionCommand(o.Key)
                });
            }

            this.PlantActions = plantActions;

        }



        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
        {
            get { return false; }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }



}
