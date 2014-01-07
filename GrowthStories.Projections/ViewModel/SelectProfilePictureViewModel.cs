
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

    public class ActionListItem
    {
        public string Title { get; set; }
        public ReactiveCommand Command { get; set; }

    }

    public class SelectProfilePictureViewModel : RoutableViewModel
    {


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public SelectProfilePictureViewModel(IUserService ctx, IMessageBus handler, IScreen host)
            : base(ctx, handler, host)
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
                {
                    var takePhotoCommand = new ReactiveCommand();
                    takePhotoCommand.Subscribe(_ => CapturePhoto());
                    var choosePhotoCommand = new ReactiveCommand();
                    choosePhotoCommand.Subscribe(_ => ChoosePhoto());

                    _Actions = new List<ActionListItem>()
                    {
                        {new ActionListItem(){
                            Title = "take photo",
                            Command = takePhotoCommand
                        }},
                        {new ActionListItem(){
                            Title = "choose from library",
                            Command = choosePhotoCommand
                        }}
                    };
                }
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
                this.RaiseAndSetIfChanged(ref _ProfilePicture, value);
            }
        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }
}