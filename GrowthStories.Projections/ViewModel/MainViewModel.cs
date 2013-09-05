
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
using System.Runtime.Serialization;

namespace Growthstories.UI.ViewModel
{
    public enum Page
    {
        MYGARDEN,
        NOTIFICATIONS,
        FRIENDS
    }

    [DataContract]
    public class MainViewModel : RoutableViewModel, IScreen
    {

        private Page _CurrentPage;
        public Page CurrentPage
        {
            get { return _CurrentPage; }
            private set
            {
                if (_CurrentPage == value)
                    return;
                _CurrentPage = value;
                UpdatePlantSelectionEnabled();
                UpdateButtons();
            }
        }

        private bool UpdatePlantSelectionEnabled()
        {
            if (CurrentPage == Page.MYGARDEN && GardenVM.IsPlantSelectionEnabled == true)
            {
                GardenVM.IsPlantSelectionEnabled = false;
                return true;
            }
            return false;
        }


        private IRoutingState _Router;
        public IRoutingState Router { get { return _Router; } private set { _Router = value; } }



        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IUserService ctx, IMessageBus bus)
            : base(ctx, bus, null)
        {
            this.HostScreen = this;
            this.Router = new RoutingState();
        }

        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _AppBarButtons, value);
            }
        }

        private GardenViewModel _GardenVM;
        public GardenViewModel GardenVM
        {
            get
            {
                if (_GardenVM == null)
                {
                    _GardenVM = new GardenViewModel(
                        this.IsInDesignMode ? Guid.NewGuid() : this.Context.CurrentUser.Id,
                        this.IsInDesignMode ? new PlantProjection(new NullUIPersistence(), null) : new PlantProjection(new NullUIPersistence(), null),
                        this.Context,
                        this.Bus,
                        this.Nav
                     );
                    _GardenVM.ButtonsRefreshed += (a, aa) =>
                    {
                        if (this.CurrentPage == Page.MYGARDEN)
                            UpdateButtons();
                    };
                }
                return _GardenVM;
            }
        }

        public void OnNavigatedTo()
        {
            this.UpdateButtons();
        }

        private void UpdateButtons()
        {
            while (this.AppBarButtons.Count > 0)
                this.AppBarButtons.RemoveAt(0);
            if (this.CurrentPage == Page.MYGARDEN)
            {
                //this.AppBarButtons = GardenVM.AppBarButtons;
                foreach (var btn in GardenVM.AppBarButtons)
                    this.AppBarButtons.Add(btn);

            }
            //else
            //{
            //    this.AppBarButtons = new ObservableCollection<ButtonViewModel>();
            //}
        }


        //public event EventHandler<SelectedPlantArgs> PlantSelected;
        private ReactiveCommand _PanoramaPageChangedCommand;
        public ReactiveCommand PanoramaPageChangedCommand
        {
            get
            {

                if (_PanoramaPageChangedCommand == null)
                {
                    _PanoramaPageChangedCommand = new ReactiveCommand();
                    _PanoramaPageChangedCommand.Subscribe(p =>
                    {
                        CurrentPage = (Page)p;
                    });
                }
                return _PanoramaPageChangedCommand;

            }
        }

        public void OnBackKeyPress(CancelEventArgs e)
        {
            if (UpdatePlantSelectionEnabled())
                e.Cancel = true;
        }


    }

    public class ButtonViewModel : MenuItemViewModel
    {
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

