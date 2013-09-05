
using Growthstories.Domain;
using Growthstories.Sync;
using ReactiveUI;
using System;

namespace Growthstories.UI.ViewModel
{

    public abstract class GSViewModelBase : ReactiveObject
    {
        public const string APPNAME = "GROWTH STORIES";

        public string AppName { get { return APPNAME; } }

        public IMessageBus Bus { get; private set; }
        public GSViewModelBase(IMessageBus bus)
        {
            this.Bus = bus;
        }

        bool DebugDesignSwitch = false;

        public bool IsInDesignMode
        {
            get
            {
                return DebugDesignSwitch ? true : DesignModeDetector.IsInDesignMode();
            }
        }

    }


    public abstract class RoutableViewModel : GSViewModelBase, IRoutableViewModel
    {

        public IUserService Context { get; private set; }
        public INavigationService Nav { get; private set; }


        protected string _PageTitle = "Undefined Title";
        private IScreen Host;
        public virtual string PageTitle { get { return _PageTitle; } }
        public abstract string UrlPathSegment { get; }
        public IScreen HostScreen { get { return Host; } protected set { Host = value; } }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public RoutableViewModel(
            IUserService ctx,
            IMessageBus bus,
            IScreen host)
            : base(bus)
        {
            this.Context = ctx;
            this.Host = host;
        }


    }





    public static class ViewModelMixins
    {

    }


}