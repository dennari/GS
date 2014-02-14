
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Growthstories.Core;
using ReactiveUI;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace Growthstories.UI.ViewModel
{

    public abstract class GSViewModelBase : ReactiveObject, IGSViewModel, IDisposable
    {
        protected readonly IGSAppViewModel App;
        public List<IDisposable> subs = new List<IDisposable>();


        public GSViewModelBase(IGSAppViewModel app)
        {

            if (app == null)
                throw new ArgumentNullException("App cannot be null");

            this.App = app;
        }

        protected async Task<IGSAggregate> SendCommand(IAggregateCommand cmd, bool GoBack = false)
        {
            var r = await App.HandleCommand(cmd);
            if (GoBack)
                NavigateBack();
            return r;
        }

        protected void Navigate(IRoutableViewModel vm)
        {
            App.Router.Navigate.Execute(vm);
        }

        protected void NavigateBack()
        {
            App.Router.NavigateBack.Execute(null);
        }

        protected IObservable<T> ListenTo<T>(Guid id = default(Guid)) where T : IEvent
        {
            var allEvents = App.Bus.Listen<IEvent>().OfType<T>();
            if (id == default(Guid))
                return allEvents;
            else
                return allEvents.Where(x => x.AggregateId == id);
        }

        public virtual void Dispose()
        {
            //this.Log().Info("disposing {0}", this.GetType().Name);
            foreach (var s in subs)
            {
                s.Dispose();
            }
        }

        ~GSViewModelBase()
        {
            var name = this.GetType().Name;
            this.Log().Info(string.Format("DESTROYING VM {0}\n", name));
        }


    }

    public abstract class RoutableViewModel : GSViewModelBase, IGSRoutableViewModel
    {

        public virtual string PageTitle { get; protected set; }
        public abstract string UrlPathSegment { get; }

        public virtual string UrlPath
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



    public class PopupViewModel : ReactiveObject, IPopupViewModel
    {
        private readonly Subject<PopupResult> Subject = new Subject<PopupResult>();
        private readonly Func<PopupResult, bool> AcceptedPredicate;

        public PopupViewModel(Func<PopupResult, bool> acceptedPredicate)
            : this()
        {
            AcceptedPredicate = acceptedPredicate;
        }


        public PopupViewModel()
        {
            DismissOnBackButton = true;
            IsLeftButtonEnabled = true;
            IsRightButtonEnabled = false;
            Type = PopupType.BASIC;
            //DismissedObservable = DismissedCommand.OfType<PopupResult>();
            AcceptedPredicate = x => x == PopupResult.LeftButton;

            DismissedObservable = Subject;
            AcceptedObservable = DismissedObservable.Where(x => AcceptedPredicate(x)).Do(x => this.Log().Info("Popup accepted: {0}", x));
        }


        public void Dismiss(PopupResult x)
        {
            this.Log().Info("Popup dismissed: {0}", x);
            Subject.OnNext(x);
        }


        public IObservable<PopupResult> DismissedObservable { get; private set; }

        public IObservable<PopupResult> AcceptedObservable { get; private set; }


        public string Caption { get; set; }

        public string Message { get; set; }

        public string LeftButtonContent { get; set; }

        public string RightButtonContent { get; set; }

        public bool IsLeftButtonEnabled { get; set; }

        public bool IsRightButtonEnabled { get; set; }

        public bool DismissOnBackButton { get; set; }

        public bool IsFullScreen { get; set; }

        public PopupType Type { get; protected set; }

        public string ProgressMessage { get; set; }
    }


    public class ProgressPopupViewModel : PopupViewModel
    {
        public ProgressPopupViewModel()
        {
            Type = PopupType.PROGRESS;
            Caption = "Synchronizing";
            ProgressMessage = "Take a breath while Growth Stories is exchanging data";
        }
    }



}