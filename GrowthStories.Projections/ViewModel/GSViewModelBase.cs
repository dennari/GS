
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace Growthstories.UI.ViewModel
{

    public interface IGSViewModel : IReactiveNotifyPropertyChanged
    {

    }

    public interface IHasAppBarButtons
    {
        ReactiveList<ButtonViewModel> AppBarButtons { get; }
    }

    public interface IHasMenuItems
    {
        ReactiveList<MenuItemViewModel> AppBarMenuItems { get; }
    }

    public abstract class GSViewModelBase : ReactiveObject, IGSViewModel
    {
        protected readonly IGSApp App;

        public GSViewModelBase(IGSApp app)
        {
            this.App = app;
        }

        protected void SendCommand(IEntityCommand cmd, bool GoBack = false)
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
                return allEvents.Where(x => x.EntityId == id);
        }


    }

    public interface IGSRoutableViewModel : IRoutableViewModel
    {
        string PageTitle { get; }
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
            IGSApp app)
            : base(app)
        {

        }


    }


    public enum SupportedPageOrientation
    {
        // Summary:
        //     Portrait orientation.
        Portrait = 1,
        //
        // Summary:
        //     Landscape orientation. Landscape supports both left and right views, but
        //     there is no way programmatically to specify one or the other.
        Landscape = 2,
        //
        // Summary:
        //     Landscape or portrait orientation.
        PortraitOrLandscape = 3,
    }

    public enum PageOrientation
    {
        // Summary:
        //     No orientation is specified.
        None = 0,
        //
        // Summary:
        //     Portrait orientation.
        Portrait = 1,
        //
        // Summary:
        //     Landscape orientation.
        Landscape = 2,
        //
        // Summary:
        //     Portrait orientation.
        PortraitUp = 5,
        //
        // Summary:
        //     Portrait orientation. This orientation is never used.
        PortraitDown = 9,
        //
        // Summary:
        //     Landscape orientation with the top of the page rotated to the left.
        LandscapeLeft = 18,
        //
        // Summary:
        //     Landscape orientation with the top of the page rotated to the right.
        LandscapeRight = 34,
    }

    public interface IControlsPageOrientation
    {
        SupportedPageOrientation SupportedOrientations { get; }
        //ReactiveCommand PageOrientationChangedCommand { get; }
    }

    public interface IControlsAppBar
    {
        ApplicationBarMode AppBarMode { get; }
        bool AppBarIsVisible { get; }
    }




    public static class ViewModelMixins
    {

    }


}