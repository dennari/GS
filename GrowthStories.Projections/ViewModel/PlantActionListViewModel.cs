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


    public sealed class PlantActionListViewModel : RoutableViewModel, IPlantActionListViewModel
    {


        private IPlantViewModel _Plant;
        public IPlantViewModel Plant
        {
            get
            {
                return _Plant;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Plant, value);
            }
        }




        public IReadOnlyReactiveList<IButtonViewModel> PlantActions { get; private set; }
        public IReactiveCommand NavigateToSelected { get; private set; }

        public PlantActionListViewModel(IPlantViewModel plant, IGSAppViewModel app)
            : base(app)
        {
            this.Plant = plant;
            var plantActions = new ReactiveList<IButtonViewModel>();

            NavigateToSelected = new ReactiveCommand();
            NavigateToSelected.OfType<PlantActionType>().Subscribe(x =>
            {
                if (this.Plant != null)
                    this.Plant.NavigateToEmptyActionCommand.Execute(x);
            });

            foreach (var o in PlantActionViewModel.ActionTypeToLabel)
            {
                plantActions.Add(new ButtonViewModel()
                {
                    Text = o.Value,
                    IconType = PlantActionViewModel.ActionTypeToIcon[o.Key],
                    Command = NavigateToSelected,
                    CommandParameter = o.Key
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
