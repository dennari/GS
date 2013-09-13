
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
using System.Reactive.Linq;
using System.Runtime.Serialization;

namespace Growthstories.UI.ViewModel
{



    public interface IMainViewModel : IGSRoutableViewModel
    {
        IGardenViewModel GardenVM { get; }
        INotificationsViewModel NotificationsVM { get; }
        IFriendsViewModel FriendsVM { get; }
    }

    [DataContract]
    public class MainViewModel : MultipageViewModel, IMainViewModel
    {

        private readonly Func<Guid, IGardenViewModel> GardenFactory;

        public MainViewModel(Func<Guid, IGardenViewModel> gardenFactory, IGSApp app)
            : base(app)
        {

            this.GardenFactory = gardenFactory;

            this.Pages.Add(this.GardenVM);
            this.Pages.Add(this.NotificationsVM);
            this.Pages.Add(this.FriendsVM);
            this.CurrentPage = this.GardenVM;


        }


        private IGardenViewModel _GardenVM;
        public IGardenViewModel GardenVM
        {
            get
            {
                return _GardenVM ?? (_GardenVM = GardenFactory(App.Context.CurrentUser.GardenId));
            }
        }

        private INotificationsViewModel _NotificationsVM;
        public INotificationsViewModel NotificationsVM
        {
            get
            {
                return _NotificationsVM ?? (_NotificationsVM = App.Resolver.GetService<INotificationsViewModel>());
            }
        }

        private IFriendsViewModel _FriendsVM;
        public IFriendsViewModel FriendsVM
        {
            get
            {
                return _FriendsVM ?? (_FriendsVM = App.Resolver.GetService<IFriendsViewModel>());
            }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class ButtonViewModel : MenuItemViewModel
    {
        public ButtonViewModel(IGSApp app)
            : base(app)
        {

        }


        #region IconUri
        private Uri uri;

        /// <summary>
        /// Gets or sets the icon URI.
        /// </summary>
        /// <value>
        /// The icon URI.
        /// </value>
        public Uri IconUri
        {
            get { return this.uri; }
            set { this.RaiseAndSetIfChanged(ref uri, value); }
        }
        #endregion
    }


    public class MenuItemViewModel : GSViewModelBase
    {

        public MenuItemViewModel(IGSApp app)
            : base(app)
        {

        }

        #region Command
        private System.Windows.Input.ICommand command;

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public System.Windows.Input.ICommand Command
        {
            get { return this.command; }
            set { this.RaiseAndSetIfChanged(ref command, value); }
        }
        #endregion

        #region CommandParameter
        private object commandParameter;

        /// <summary>
        /// Gets or sets the command's parameter.
        /// </summary>
        /// <value>
        /// The command's parameter.
        /// </value>
        public object CommandParameter
        {
            get { return this.commandParameter; }
            set { this.RaiseAndSetIfChanged(ref commandParameter, value); }
        }
        #endregion

        #region Text
        private string text;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get { return this.text; }
            set { this.RaiseAndSetIfChanged(ref text, value); }
        }
        #endregion
    }
}

