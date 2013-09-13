
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{

    public interface IPlantViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsAppBar
    {
        Guid Id { get; }
        string Name { get; }
        string ProfilePicturePath { get; }
    }

    public class PlantViewModel : RoutableViewModel, IPlantViewModel
    {


        protected ReactiveList<ActionBase> _Actions;
        public IList<ActionBase> Actions
        {
            get
            {
                if (_Actions == null)
                {
                    _Actions = new ReactiveList<ActionBase>();
                    //LoadActions();
                    //a();
                    this.GetActionsCommand.Execute(Tuple.Create(State.UserId, State.Id));

                }
                return _Actions;
            }
        }


        public ReactiveList<ButtonViewModel> AppBarButtons { get; protected set; }

        public Guid Id { get { return State.Id; } }
        public string Name { get { return State.Name; } }
        public string ProfilePicturePath { get { return State.ProfilepicturePath; } }

        public ApplicationBarMode AppBarMode { get { return ApplicationBarMode.DEFAULT; } }
        public bool AppBarIsVisible { get { return true; } }

        private readonly Func<object, IEnumerable<ActionBase>> ActionFactory;

        protected PlantState State;

        public PlantViewModel()
            : base(null)
        {

        }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PlantViewModel(PlantState state, Func<object, IEnumerable<ActionBase>> actionFactory, IGSApp app)
            : base(app)
        {
            //this.ActionProjection = actionProjection;
            //this.ActionProjection.EventHandled += this.ActionHandled;
            //this.Actions = new ObservableCollection<ActionBase>();
            this.State = state;
            this.AppBarButtons = new ReactiveList<ButtonViewModel>();
            this.ActionFactory = actionFactory;


            this.GetActionsCommand = new ReactiveCommand();
            this.GetActionsPipe = this.GetActionsCommand
                .RegisterAsyncFunction((id) => actionFactory(id), RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            this.GetActionsPipe.Subscribe(x => this._Actions.AddRange(x));

        }





        private ReactiveCommand _AddCommentCommand;
        public ReactiveCommand AddCommentCommand
        {
            get
            {

                if (_AddCommentCommand == null)
                {
                    _AddCommentCommand = new ReactiveCommand();
                    _AddCommentCommand.Subscribe((note) =>
                    {
                        this.Add(new Comment(App.Context.CurrentUser.Id, this.State.Id, (string)note));
                    });
                }
                return _AddCommentCommand;

            }
        }

        private ReactiveCommand _AddPhotoCommand;
        public ReactiveCommand AddPhotoCommand
        {
            get
            {

                if (_AddPhotoCommand == null)
                {
                    _AddPhotoCommand = new ReactiveCommand();
                    _AddPhotoCommand.Subscribe((photo) =>
                    {
                        this.Add(new Photograph(App.Context.CurrentUser.Id, this.State.Id, "", photo as Uri));
                    });
                }
                return _AddPhotoCommand;

            }
        }


        private ReactiveCommand _AddFertilizerCommand;
        public ReactiveCommand AddFertilizerCommand
        {
            get
            {

                if (_AddFertilizerCommand == null)
                {
                    _AddFertilizerCommand = new ReactiveCommand();
                    _AddFertilizerCommand.Subscribe(_ =>
                    {
                        this.Add(new Fertilize(App.Context.CurrentUser.Id, this.State.Id, ""));
                    });
                }
                return _AddFertilizerCommand;

            }
        }

        private ReactiveCommand _AddWaterCommand;
        private ReactiveCommand GetActionsCommand;
        private IObservable<IEnumerable<ActionBase>> GetActionsPipe;
        public ReactiveCommand AddWaterCommand
        {
            get
            {

                if (_AddWaterCommand == null)
                {
                    _AddWaterCommand = new ReactiveCommand();
                    _AddWaterCommand.Subscribe(_ =>
                    {
                        this.Add(new Water(App.Context.CurrentUser.Id, this.State.Id, ""));
                    });
                }
                return _AddWaterCommand;

            }
        }


        public void Add(Comment command)
        {
            App.Bus.Handle(command);
        }

        public void Add(Fertilize command)
        {
            App.Bus.Handle(command);
        }

        public void Add(Water command)
        {
            App.Bus.Handle(command);

        }

        public void Add(Photograph command)
        {
            App.Bus.Handle(command);

        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }


    }


    public class PlantViewModelDesign : PlantViewModel
    {

        public new IList<ActionBase> Actions
        {
            get
            {
                return new List<ActionBase>()
                {
                    new Watered(State.UserId,State.Id,"Watered"),
                    new Fertilized(State.UserId,State.Id,"Fertilized"),
                    new Commented(State.UserId,State.Id,"Commented")

                };
            }
        }

        public PlantViewModelDesign()
            : base()
        {
            this.State = new PlantState(new PlantCreated(Guid.NewGuid(), "Jare", Guid.NewGuid()));
        }
    }
}