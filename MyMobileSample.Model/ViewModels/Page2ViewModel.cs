using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMobileSample.Model.ViewModels
{
    public class Page2ViewModel : ReactiveObject, IMyVM
    {
        public const string URI = "Page2";


        public string UrlPathSegment
        {
            get { return URI; }
        }

        public IScreen HostScreen { get; private set; }


        public Page2ViewModel(IScreen host, IThreadIdFactory threadIdFactory, IMessageBus bus)
        {
            this.HostScreen = host;
            this.ThreadIdFactory = threadIdFactory;
            this.Bus = bus;
            this.Bus.Listen<IEvent>().Subscribe(e => this.Events.Add(e));
        }

        private ReactiveList<IEvent> _Events;
        public ReactiveList<IEvent> Events 
        {
            get
            {
                return _Events ?? (_Events = new ReactiveList<IEvent>());
            }
        }

        private ReactiveList<IAppBarButton> _AppBarButtons;
        private IThreadIdFactory ThreadIdFactory;
        private IMessageBus Bus;
        public ReactiveList<IAppBarButton> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                {
                    _AppBarButtons = new ReactiveList<IAppBarButton>();
                }
                return _AppBarButtons;
            }
        }


        public void SetupAppBarButtons()
        {
            this.AppBarButtons.Add(new ButtonViewModel()
            {
                Text = "Page1",
                Command = HostScreen.Router.NavigateCommandFor<Page1ViewModel>(),
                IconUri = Icons.IconUri[IconType.ADD]
            });
        }

        public void ClearAppBarButtons()
        {
            this.AppBarButtons.RemoveRange(0, this.AppBarButtons.Count);

        }


    }
}
