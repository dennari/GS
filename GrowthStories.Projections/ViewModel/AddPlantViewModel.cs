
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;


namespace Growthstories.UI.ViewModel
{

    public interface IAddPlantViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }
    public class AddPlantViewModel : RoutableViewModel, IAddPlantViewModel
    {


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public AddPlantViewModel(IGSApp app)
            : base(app)
        {

        }


        private ReactiveCommand _CreatePlantCommand;
        public ReactiveCommand CreatePlantCommand
        {
            get
            {

                if (_CreatePlantCommand == null)
                {
                    _CreatePlantCommand = new ReactiveCommand();
                    _CreatePlantCommand.RegisterAsyncAction(param =>
                    {
                        App.Bus.SendMessage<IEntityCommand>(
                            new CreatePlant(
                                this.Id,
                                this.Name,
                                App.Context.CurrentUser.Id
                             )
                             {
                                 ProfilepicturePath = this.ProfilepicturePath
                             }
                         );
                        App.Bus.SendMessage<IEntityCommand>(
                            new AddPlant(
                                App.Context.CurrentUser.GardenId,
                                this.Id,
                                this.Name
                             )
                        );
                    });
                    _CreatePlantCommand.Subscribe(param =>
                    {
                        //this.Name = "";
                        //Nav.GoBack();
                    });

                }
                return _CreatePlantCommand;

            }
        }

        private ReactiveCommand _ChooseProfilePictureCommand;
        public ReactiveCommand ChooseProfilePictureCommand
        {
            get
            {

                if (_ChooseProfilePictureCommand == null)
                {
                    _ChooseProfilePictureCommand = new ReactiveCommand();
                    _ChooseProfilePictureCommand.Subscribe(param => ChoosePhoto());

                }
                return _ChooseProfilePictureCommand;

            }
        }

        protected virtual void ChoosePhoto()
        {

        }

        protected string _PageTitle = "add plant";
        public override string PageTitle { get { return _PageTitle; } }


        protected string _ProfilePictureButtonText = "select";
        public string ProfilePictureButtonText
        {
            get
            {
                return _ProfilePictureButtonText;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ProfilePictureButtonText, value);
            }
        }

        protected string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Name, value);
            }
        }


        protected Guid _Id;
        public Guid Id
        {
            get
            {
                return _Id == default(Guid) ? Guid.NewGuid() : _Id;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Id, value);
            }
        }

        protected string _ProfilepicturePath;
        public string ProfilepicturePath
        {
            get
            {
                return _ProfilepicturePath;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ProfilepicturePath, value);
            }
        }

        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                    _AppBarButtons = new ReactiveList<ButtonViewModel>()
                    {
                        new ButtonViewModel(App)
                        {
                            Text = "add",
                            //IconUri = Nav.IconUri[IconType.CHECK],
                            Command = CreatePlantCommand
                        }
                    };
                return _AppBarButtons;
            }
        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }
    }
}