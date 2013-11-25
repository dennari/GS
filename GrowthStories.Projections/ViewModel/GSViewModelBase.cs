
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
            App.HandleCommand(cmd);
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

        public string UrlPath
        {
            get
            {
                return string.Format("/MainWindow.xaml?{0}", this.UrlPathSegment);
            }
        }

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


    public sealed class ButtonViewModel : MenuItemViewModel, IButtonViewModel
    {
        public ButtonViewModel(IGSAppViewModel app)
            : base(app)
        {

        }

        public ButtonViewModel()
        {

        }


        /// <summary>
        /// Gets or sets the icon URI.
        /// </summary>
        /// <value>
        /// The icon URI.
        /// </value>
        private IconType _IconType;
        public IconType IconType
        {
            get { return this._IconType; }
            set { this.RaiseAndSetIfChanged(ref _IconType, value); }
        }
    }




    public class MenuItemViewModel : ReactiveObject, IMenuItemViewModel
    {

        public MenuItemViewModel(IGSAppViewModel app)
        {

        }

        public MenuItemViewModel()
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