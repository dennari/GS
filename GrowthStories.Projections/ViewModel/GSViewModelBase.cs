
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace Growthstories.UI.ViewModel
{

    public abstract class GSViewModelBase : ReactiveObject, IGSViewModel
    {
        public IGSAppViewModel App { get; protected set; }

        public GSViewModelBase(IGSAppViewModel app)
        {
            this.App = app;
        }

        protected void SendCommand(IAggregateCommand cmd, bool GoBack = false)
        {
            App.Bus.SendCommand(cmd);
            if (GoBack)
                App.Router.NavigateBack.Execute(null);
        }

        protected void Navigate(IRoutableViewModel vm)
        {
            App.Router.Navigate.Execute(vm);
        }

        protected IObservable<T> ListenTo<T>(Guid id = default(Guid)) where T : IEvent
        {
            var allEvents = App.Bus.Listen<IEvent>().OfType<T>();
            if (id == default(Guid))
                return allEvents;
            else
                return allEvents.Where(x => x.AggregateId == id);
        }


    }

    public abstract class RoutableViewModel : GSViewModelBase, IGSRoutableViewModel
    {

        public virtual string PageTitle { get; protected set; }
        public abstract string UrlPathSegment { get; }

        public IScreen HostScreen { get { return App; } }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public RoutableViewModel(
            IGSAppViewModel app)
            : base(app)
        {

        }


    }


    public class ButtonViewModel : MenuItemViewModel, IButtonViewModel
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




    public class MenuItemViewModel : GSViewModelBase, IMenuItemViewModel
    {

        public MenuItemViewModel(IGSAppViewModel app)
            : base(app)
        {

        }

        #region Command
        private IReactiveCommand command;

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public IReactiveCommand Command
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