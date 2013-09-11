
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System;
using System.Collections.Generic;

namespace Growthstories.UI.ViewModel
{

    public class AppViewModel : ReactiveObject, IScreen, IHasAppBarButtons, IControlsAppBar
    {


        IRoutingState _Router;
        public IRoutingState Router
        {
            get { return _Router ?? (_Router = new RoutingState()); }
        }


        private bool DebugDesignSwitch = false;

        protected readonly IMutableDependencyResolver Resolver;


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public AppViewModel()
        {

            var resolver = RxApp.MutableResolver;

            resolver.RegisterConstant(this, typeof(IScreen));
            resolver.RegisterConstant(this.Router, typeof(IRoutingState));
            this.Resolver = resolver;

            this.Router.CurrentViewModel
                .OfType<IHasAppBarButtons>()
                .Select(x => x.WhenAny(y => y.AppBarButtons, y => y.GetValue()).StartWith(x.AppBarButtons))
                .Switch()
                .Subscribe(x => UpdateAppBar(x));

            this.Router.CurrentViewModel
                .OfType<IControlsAppBar>()
                .Select(x => x.WhenAny(y => y.AppBarMode, y => y.GetValue()).StartWith(x.AppBarMode))
                .Switch()
                .ToProperty(this, x => x.AppBarMode, out this._AppBarMode, GSApp.APPBAR_MODE_DEFAULT);

            this.Router.CurrentViewModel
                 .OfType<IControlsAppBar>()
                 .Select(x => x.WhenAny(y => y.AppBarIsVisible, y => y.GetValue()).StartWith(x.AppBarIsVisible))
                 .Switch()
                 .ToProperty(this, x => x.AppBarIsVisible, out this._AppBarIsVisible, true);

            //this.Router.CurrentViewModel
            //    .OfType<IPanoramaViewModel>()
            //    .Subscribe(x =>
            //    {
            //        x
            //          .WhenAny(y => y.AppBarButtons, y => y.Value)
            //          .Subscribe(z => UpdateAppBar(z));
            //    });

            //this.AppBarButtons.CountChanged.Subscribe(x => AppBarIsVisible = x > 0);

        }

        protected ObservableAsPropertyHelper<bool> _AppBarIsVisible;
        public bool AppBarIsVisible
        {
            get { return _AppBarIsVisible.Value; }
        }

        protected ObservableAsPropertyHelper<string> _AppBarMode;
        public string AppBarMode
        {
            get { return _AppBarMode.Value; }
        }

        private void UpdateAppBar(IList<ButtonViewModel> x)
        {
            this.AppBarButtons.RemoveRange(0, this.AppBarButtons.Count);
            this.AppBarButtons.AddRange(x);
        }


        protected ReactiveList<ButtonViewModel> _AppBarButtons = new ReactiveList<ButtonViewModel>();
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons;
            }
        }

    }


}

