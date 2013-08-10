using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Ninject;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{
    public enum Page
    {
        MYGARDEN,
        NOTIFICATIONS,
        FRIENDS
    }

    public class MainViewModel : GSViewModelBase, IPanoramaPage
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

        private IKernel Kernel;

        //public ObservableCollection<ButtonViewModel> AppBarButtons { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IMessenger messenger, IUserService ctx, IMessageBus handler, INavigationService nav, IKernel kernel)
            : base(messenger, ctx, handler, nav)
        {
            this._AppBarButtons = new ObservableCollection<ButtonViewModel>();
            this.Kernel = kernel;
            this.CurrentPage = Page.MYGARDEN;

        }

        protected ObservableCollection<ButtonViewModel> _AppBarButtons;
        public ObservableCollection<ButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons;
            }
            set
            {
                Set(() => AppBarButtons, ref _AppBarButtons, value);
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
                        this.IsInDesignMode ? new PlantProjection(new NullUIPersistence(), null) : this.Kernel.Get<PlantProjection>(),
                        this.MessengerInstance,
                        this.Context,
                        this.Handler,
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
        private RelayCommand<int> _PanoramaPageChangedCommand;
        public RelayCommand<int> PanoramaPageChangedCommand
        {
            get
            {

                if (_PanoramaPageChangedCommand == null)
                    _PanoramaPageChangedCommand = new RelayCommand<int>(p =>
                    {
                        CurrentPage = (Page)p;
                    });
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
            get
            {
                return this.uri;
            }
            set
            {
                if (this.uri != value)
                {
                    this.uri = value;
                    RaisePropertyChanged("IconUri");
                }
            }
        }
        #endregion
    }


    public class MenuItemViewModel : ViewModelBase
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
            set
            {
                if (this.command != value)
                {
                    this.command = value;
                    RaisePropertyChanged("Command");
                }
            }
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
            set
            {
                if (this.commandParameter != value)
                {
                    this.commandParameter = value;
                    RaisePropertyChanged("CommandParameter");
                }
            }
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
            set
            {
                if (this.text != value)
                {
                    this.text = value;
                    RaisePropertyChanged("Text");
                }
            }
        }
        #endregion
    }
}

