using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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


    public class AddPlantViewModel : GSViewModelBase
    {


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public AddPlantViewModel(IMessenger messenger, IUserService ctx, IMessageBus handler, INavigationService nav)
            : base(messenger, ctx, handler, nav)
        {

        }


        private RelayCommand _CreatePlantCommand;
        public RelayCommand CreatePlantCommand
        {
            get
            {

                if (_CreatePlantCommand == null)
                    _CreatePlantCommand = new RelayCommand(() =>
                    {
                        Handler.Handle(
                            new CreatePlant(
                                this.Id,
                                this.Name,
                                this.Context.CurrentUser.Id
                             )
                             {
                                 ProfilepicturePath = this.ProfilepicturePath
                             }
                         );
                        Handler.Handle(
                            new AddPlant(
                                this.Context.CurrentUser.GardenId,
                                this.Id,
                                this.Name
                             )
                        );
                        this.Name = "";
                        Nav.GoBack();
                    });
                return _CreatePlantCommand;

            }
        }

        private RelayCommand _ChooseProfilePictureCommand;
        public RelayCommand ChooseProfilePictureCommand
        {
            get
            {

                if (_ChooseProfilePictureCommand == null)
                    _ChooseProfilePictureCommand = new RelayCommand(() =>
                    {
                        ChoosePhoto();
                    });
                return _ChooseProfilePictureCommand;

            }
        }

        protected virtual void ChoosePhoto()
        {

        }

        protected new string _PageTitle = "add plant";
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
                Set(() => ProfilePictureButtonText, ref _ProfilePictureButtonText, value);
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
                Set(() => Name, ref _Name, value);
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
                Set(() => Id, ref _Id, value);
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
                Set(() => ProfilepicturePath, ref _ProfilepicturePath, value);
            }
        }

        protected ObservableCollection<ButtonViewModel> _AppBarButtons;
        public ObservableCollection<ButtonViewModel> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                    _AppBarButtons = new ObservableCollection<ButtonViewModel>()
                    {
                        new ButtonViewModel()
                        {
                            Text = "add",
                            IconUri = Nav.IconUri[IconType.CHECK],
                            Command = CreatePlantCommand
                        }
                    };
                return _AppBarButtons;
            }
        }

    }
}