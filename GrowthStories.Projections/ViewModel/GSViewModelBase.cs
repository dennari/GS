
using Growthstories.Domain;
using Growthstories.Sync;
using ReactiveUI;
using System;

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





    public static class ViewModelMixins
    {

    }


}