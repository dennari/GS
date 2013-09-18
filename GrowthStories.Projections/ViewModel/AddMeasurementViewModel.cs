using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;

namespace Growthstories.UI.ViewModel
{
    public interface IAddMeasurementViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }

    public class AddMeasurementViewModel : RoutableViewModel, IAddMeasurementViewModel
    {

        protected PlantState State;

        public AddMeasurementViewModel()
            : base(null)
        {
        }
        public AddMeasurementViewModel(PlantState state, IGSApp app)
            : base(app)
        {
            //this.ActionProjection = actionProjection;
            //this.ActionProjection.EventHandled += this.ActionHandled;
            //this.Actions = new ObservableCollection<ActionBase>();
            this.State = state;

        }
        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                    _AppBarButtons = new ReactiveList<ButtonViewModel>()
                    {
                        new ButtonViewModel(null)
                        {
                            Text = "add",
                            IconUri = App.IconUri[IconType.CHECK],
                            Command = AddCommand
                        }
                    };
                return _AppBarButtons;
            }
        }

        protected string _Note;
        public string Note
        {
            get
            {
                return _Note;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Note, value);
            }
        }

        private ReactiveCommand _AddCommand;
        public ReactiveCommand AddCommand
        {
            get
            {

                if (_AddCommand == null)
                {
                    _AddCommand = new ReactiveCommand();
                    _AddCommand.Subscribe(_ =>
                    {
                        App.Bus.SendCommand(new Measure(this.State.UserId, this.State.Id, this.Note, MeasurementType.LENGTH, 23.46));
                        App.Router.NavigateBack.Execute(null);
                    });
                }
                return _AddCommand;

            }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }


    public class AddMeasurementViewModelDesign : AddMeasurementViewModel
    {
        public AddMeasurementViewModelDesign()
        {
            this.State = new PlantState(new PlantCreated(Guid.NewGuid(),"Jari",Guid.NewGuid()));
        }
    }
}
