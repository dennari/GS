using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{

    public class ActionListItem
    {
        public string Title { get; set; }
        public RelayCommand Command { get; set; }

    }

    public class SelectProfilePictureViewModel : GSViewModelBase
    {


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public SelectProfilePictureViewModel(IMessenger messenger, IUserService ctx, IDispatchCommands handler, INavigationService nav)
            : base(messenger, ctx, handler, nav)
        {

        }

        protected new string _PageTitle = "profile picture";
        public override string PageTitle { get { return _PageTitle; } }

        private IList<ActionListItem> _Actions;
        public IList<ActionListItem> Actions
        {
            get
            {
                if (_Actions == null)
                    _Actions = new List<ActionListItem>()
                    {
                        {new ActionListItem(){
                            Title = "take photo",
                            Command = new RelayCommand(()=> CapturePhoto())
                        }},
                        {new ActionListItem(){
                            Title = "choose from library",
                            Command = new RelayCommand(()=> ChoosePhoto())
                        }}
                    };
                return _Actions;
            }
        }

        protected virtual void CapturePhoto()
        {

        }

        protected virtual void ChoosePhoto()
        {

        }

        protected string _ProfilePicture;
        public string ProfilePicture
        {
            get
            {
                return _ProfilePicture;
            }
            set
            {
                Set(() => ProfilePicture, ref _ProfilePicture, value);
            }
        }

    }
}