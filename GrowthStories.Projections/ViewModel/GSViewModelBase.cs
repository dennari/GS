using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Growthstories.Domain;
using Growthstories.Sync;
using System;

namespace Growthstories.UI.ViewModel
{

    public class GSViewModelBase : ViewModelBase
    {
        public IUserService Context { get; private set; }
        public INavigationService Nav { get; private set; }
        public IDispatchCommands Handler { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public GSViewModelBase(IMessenger messenger, IUserService ctx, IDispatchCommands handler, INavigationService nav)
            : base(messenger)
        {
            this.Handler = handler;
            this.Context = ctx;
            this.Nav = nav;
        }



    }

}