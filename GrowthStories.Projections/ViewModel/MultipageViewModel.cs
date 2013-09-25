using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{



    public interface IMultipageViewModel : IGSRoutableViewModel
    {
        IGSViewModel CurrentPage { get; }
        ReactiveList<IGSViewModel> Pages { get; }
        ReactiveCommand PageChangedCommand { get; }
    }

    public abstract class MultipageViewModel : RoutableViewModel, IMultipageViewModel, IHasAppBarButtons, IHasMenuItems, IControlsAppBar
    {
        public MultipageViewModel(IGSApp app)
            : base(app)
        {
            this.PageChangedCommand
                .Select(x => TryGetPage(x))
                .Subscribe(x => this.CurrentPage = x);


            var currentPageChanged = this.WhenAny(x => x.CurrentPage, x => x.GetValue());


            //this.ObservableForProperty(x => x.CurrentPage.)


            currentPageChanged
                .OfType<IHasAppBarButtons>()
                .Select(x => x.WhenAny(y => y.AppBarButtons, y => y.GetValue()).StartWith(x.AppBarButtons))
                .Switch()
                .ToProperty(this, x => x.AppBarButtons, out this._AppBarButtons, new ReactiveList<ButtonViewModel>());

            currentPageChanged
                .OfType<IHasMenuItems>()
                .Select(x => x.WhenAny(y => y.AppBarMenuItems, y => y.GetValue()).StartWith(x.AppBarMenuItems))
                .Switch()
                .ToProperty(this, x => x.AppBarMenuItems, out this._AppBarMenuItems, new ReactiveList<MenuItemViewModel>());

            currentPageChanged
                .OfType<IControlsAppBar>()
                .Select(x => x.WhenAny(y => y.AppBarMode, y => y.GetValue()).StartWith(x.AppBarMode))
                .Switch()
                .ToProperty(this, x => x.AppBarMode, out this._AppBarMode, ApplicationBarMode.DEFAULT);

            currentPageChanged
                 .OfType<IControlsAppBar>()
                 .Select(x => x.WhenAny(y => y.AppBarIsVisible, y => y.GetValue()).StartWith(x.AppBarIsVisible))
                 .Switch()
                 .ToProperty(this, x => x.AppBarIsVisible, out this._AppBarVisibility, true);


        }

        public IGSViewModel TryGetPage(object x)
        {
            var y = x as IGSViewModel;
            if (y != null && this.Pages.Contains(y))
            {
                return y;
            }

            try
            {
                int i = (int)x;
                if (i < this.Pages.Count)
                {
                    return this.Pages[i];
                }
            }
            catch
            {

            }

            return null;
        }

        private ReactiveCommand _PageChangedCommand;
        public ReactiveCommand PageChangedCommand
        {
            get
            {

                if (_PageChangedCommand == null)
                {
                    _PageChangedCommand = new ReactiveCommand(Observable.Return(true), false, null, true);
                }
                return _PageChangedCommand;

            }
        }

        protected IGSViewModel _CurrentPage;
        public IGSViewModel CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                this.RaiseAndSetIfChanged(ref _CurrentPage, value);
            }
        }

        //protected ReactiveList<ButtonViewModel> _AppBarButtons;
        //public ReactiveList<ButtonViewModel> AppBarButtons
        //{
        //    get
        //    {
        //        return _AppBarButtons;
        //    }
        //    set
        //    {
        //        this.RaiseAndSetIfChanged(ref _AppBarButtons, value);
        //    }
        //}

        protected ObservableAsPropertyHelper<ReactiveList<ButtonViewModel>> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get { return _AppBarButtons.Value; }
        }

        protected ObservableAsPropertyHelper<ReactiveList<MenuItemViewModel>> _AppBarMenuItems;
        public ReactiveList<MenuItemViewModel> AppBarMenuItems
        {
            get { return _AppBarMenuItems.Value; }
        }

        protected ReactiveList<IGSViewModel> _Pages;
        public ReactiveList<IGSViewModel> Pages
        {
            get { return _Pages ?? (_Pages = new ReactiveList<IGSViewModel>()); }
        }

        protected ObservableAsPropertyHelper<ApplicationBarMode> _AppBarMode;
        public ApplicationBarMode AppBarMode
        {
            get { return _AppBarMode.Value; }
        }

        protected ObservableAsPropertyHelper<bool> _AppBarVisibility;
        public bool AppBarIsVisible
        {
            get { return _AppBarVisibility.Value; }
        }
    }
}
