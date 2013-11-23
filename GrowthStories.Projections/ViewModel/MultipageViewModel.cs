﻿using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{




    public abstract class MultipageViewModel : RoutableViewModel, IMultipageViewModel, IHasAppBarButtons, IHasMenuItems, IControlsAppBar
    {
        public MultipageViewModel(IGSAppViewModel app)
            : base(app)
        {
            this.PageChangedCommand
                .Select(x => TryGetPage(x))
                .Subscribe(x => this.SelectedPage = x);


            var currentPageChanged = this.WhenAny(x => x.SelectedPage, x => x.GetValue());


            //this.ObservableForProperty(x => x.CurrentPage.)


            currentPageChanged
                .OfType<IHasAppBarButtons>()
                .Select(x => x.WhenAny(y => y.AppBarButtons, y => y.GetValue()).StartWith(x.AppBarButtons))
                .Switch()
                .ToProperty(this, x => x.AppBarButtons, out this._AppBarButtons, (IReadOnlyReactiveList<IButtonViewModel>)(new ReactiveList<IButtonViewModel>()));

            currentPageChanged
                .OfType<IHasMenuItems>()
                .Select(x => x.WhenAny(y => y.AppBarMenuItems, y => y.GetValue()).StartWith(x.AppBarMenuItems))
                .Switch()
                .ToProperty(this, x => x.AppBarMenuItems, out this._AppBarMenuItems, (IReadOnlyReactiveList<IMenuItemViewModel>)(new ReactiveList<IMenuItemViewModel>()));

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
            if (y != null && this._Pages.Contains(y))
            {
                return y;
            }

            try
            {
                int i = (int)x;
                if (i < this._Pages.Count)
                {
                    return this._Pages[i];
                }
            }
            catch
            {

            }

            return null;
        }

        private ReactiveCommand _PageChangedCommand;
        public IReactiveCommand PageChangedCommand
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

        protected IGSViewModel _SelectedPage;
        public IGSViewModel SelectedPage
        {
            get { return _SelectedPage; }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedPage, value);
            }
        }


        public object SelectedItem
        {
            get { return _SelectedPage; }
            set
            {
                if (value != null)
                {
                    var v = value as IGSViewModel;
                    if (v != null)
                    {
                        SelectedPage = v;
                    }
                }
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

        protected ObservableAsPropertyHelper<IReadOnlyReactiveList<IButtonViewModel>> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get { return _AppBarButtons.Value; }
        }

        protected ObservableAsPropertyHelper<IReadOnlyReactiveList<IMenuItemViewModel>> _AppBarMenuItems;
        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get { return _AppBarMenuItems.Value; }
        }

        protected ReactiveList<IGSViewModel> __Pages;
        protected ReactiveList<IGSViewModel> _Pages { get { return __Pages ?? (__Pages = new ReactiveList<IGSViewModel>()); } }

        public IReadOnlyReactiveList<IGSViewModel> Items
        {
            get { return _Pages; }
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
