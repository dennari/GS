
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
        FriendsViewModel FriendsVM { get; }
    }

    [DataContract]
    public class MainViewModel : MultipageViewModel, IMainViewModel, IControlsSystemTray, IControlsProgressIndicator
    {



        public MainViewModel(IGSAppViewModel app, IGardenViewModel gvm = null)
            : base(app)
        {

            if (gvm != null)
                this.GardenVM = gvm;


            this.Pages.Add(this.GardenVM);
            this.Pages.Add(this.NotificationsVM);
            this.Pages.Add(this.FriendsVM);

            this.CurrentPage = this.GardenVM;

            //app.Gardens
            //    .Where(x => x.UserState.Id == app.Context.CurrentUser.Id)
            //    .Subscribe(x =>
            //    {
            //        this.GardenVM = x;
            //    });

        }


        private IGardenViewModel _GardenVM;
        public IGardenViewModel GardenVM
        {
            get
            {
                return _GardenVM;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _GardenVM, value);
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

        private FriendsViewModel _FriendsVM;
        public FriendsViewModel FriendsVM
        {
            get
            {
                return _FriendsVM ?? (_FriendsVM = App.Resolver.GetService<FriendsViewModel>());
            }
        }

        private TestingViewModel _TestingVM;
        public TestingViewModel TestingVM
        {
            get
            {
                var vm = _TestingVM ?? (_TestingVM = new TestingViewModel(App));

                //vm.AddTestDataCommandAsync.Subscribe(_ => GardenVM = App.GardenFactory(App.Context.CurrentUser.Id));
                //vm.ClearDBCommandAsync.Subscribe(_ => GardenVM = App.GardenFactory(App.Context.CurrentUser.Id));

                return vm;
            }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public bool SystemTrayIsVisible
        {
            get { return false; }
        }

        public bool ProgressIndicatorIsVisible
        {
            get { return false; }
        }
    }

    public class TestingViewModel : GSViewModelBase
    {
        public TestingViewModel(IGSAppViewModel app)
            : base(app)
        {
            this.AddTestDataCommand = new ReactiveCommand();
            this.AddTestDataCommandAsync = this.AddTestDataCommand.RegisterAsyncTask(async (x) => await this.App.AddTestData());
            this.ClearDBCommand = new ReactiveCommand();
            this.ClearDBCommandAsync = this.ClearDBCommand.RegisterAsyncTask(async (x) => await this.App.ClearDB());
            this.SyncCommand = new ReactiveCommand();
            //this.SyncCommandAsync = this.SyncCommand.RegisterAsyncTask(async (x) => await this.App.Synchronize());


        }

        public ReactiveCommand AddTestDataCommand { get; protected set; }
        public ReactiveCommand ClearDBCommand { get; protected set; }
        public ReactiveCommand SyncCommand { get; protected set; }
        //public ReactiveCommand ClearDBCommandAsync { get; protected set; }


        public IObservable<System.Reactive.Unit> ClearDBCommandAsync { get; protected set; }

        public IObservable<System.Reactive.Unit> AddTestDataCommandAsync { get; protected set; }

        public IObservable<SyncResult> SyncCommandAsync { get; protected set; }
    }

    public class ButtonViewModel : MenuItemViewModel
    {
        public ButtonViewModel(IGSAppViewModel app)
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

        public MenuItemViewModel(IGSAppViewModel app)
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

        #region IsEnabled
        private bool _IsEnabled = true;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public bool IsEnabled
        {
            get { return this._IsEnabled; }
            set { this.RaiseAndSetIfChanged(ref _IsEnabled, value); }
        }
        #endregion
    }
}

