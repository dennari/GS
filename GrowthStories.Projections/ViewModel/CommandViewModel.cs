using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
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



    public abstract class CommandViewModel : RoutableViewModel, ICommandViewModel, IControlsAppBar
    {
        protected string _TopTitle;
        public string TopTitle { get { return _TopTitle; } protected set { this.RaiseAndSetIfChanged(ref _TopTitle, value); } }

        protected string _Title;
        public string Title { get { return _Title; } protected set { this.RaiseAndSetIfChanged(ref _Title, value); } }

        public CommandViewModel(IGSAppViewModel app)
            : base(app)
        { }

        protected ReactiveList<IButtonViewModel> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                    _AppBarButtons = new ReactiveList<IButtonViewModel>()
                    {
                        new ButtonViewModel(null)
                        {
                            Text = "save",
                            IconType = IconType.CHECK,
                            Command = AddCommand
                        }
                    };
                return _AppBarButtons;
            }
        }
        private ReactiveCommand _AddCommand;
        public IReactiveCommand AddCommand
        {
            get
            {

                if (_AddCommand == null)
                {
                    _AddCommand = new ReactiveCommand(this.CanExecute == null ? Observable.Return(true) : this.CanExecute, false);
                    _AddCommand.Subscribe(this.AddCommandSubscription);

                }
                return _AddCommand;

            }
        }

        public virtual void AddCommandSubscription(object p)
        {

        }

        public IObservable<bool> CanExecute { get; protected set; }


        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }


}
