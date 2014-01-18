using System;
using System.Reactive.Linq;
using Growthstories.Domain.Entities;
using ReactiveUI;
//using EventStore.Logging;

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

        public const string ACTIONLIST_ID = "actionlist";


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
                    this.Plant.NavigateToEmptyActionCommand.Execute(Tuple.Create(x, ACTIONLIST_ID));
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
