using ReactiveUI;
using System.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Linq;

namespace MyMobileSample.Model.ViewModels
{

    public class ButtonViewModel : MenuItemViewModel, IAppBarButton
    {
        #region IconUri
        private Uri uri;
        public Uri IconUri { get { return this.uri; } set { this.RaiseAndSetIfChanged(ref uri, value); } }

        #endregion
    }


    public class MenuItemViewModel : ReactiveObject
    {
        #region Command
        private ICommand command;
        public ICommand Command { get { return this.command; } set { this.RaiseAndSetIfChanged(ref command, value); } }
        #endregion

        #region CommandParameter
        private object commandParameter;
        public object CommandParameter { get { return this.commandParameter; } set { this.RaiseAndSetIfChanged(ref commandParameter, value); } }

        #endregion

        #region Text
        private string text;
        public string Text { get { return this.text; } set { this.RaiseAndSetIfChanged(ref text, value); } }

        #endregion
    }

    public interface IAppBarButton
    {
        string Text { get; }
        ICommand Command { get; }
        object CommandParameter { get; }
        Uri IconUri { get; }
    }

    public interface IMyVM : IRoutableViewModel
    {
        ReactiveList<IAppBarButton> AppBarButtons { get; }
        void SetupAppBarButtons();
        void ClearAppBarButtons();

    }

    public interface IEvent
    {

    }

    public class MyEvent : IEvent
    {
        string Content;

        public MyEvent(string content)
        {
            this.Content = content;
        }

        public override string ToString()
        {
            return string.Format("MyEvent: {0}", this.Content);
        }
    }


    public enum IconType
    {
        ADD,
        CHECK,
        CANCEL,
        DELETE,
        CHECK_LIST
    }

    public static class Icons
    {
        private static IDictionary<IconType, Uri> _IconUri = new Dictionary<IconType, Uri>()
        {
            {IconType.ADD,new Uri("/Assets/Icons/appbar.add.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK,new Uri("/Assets/Icons/appbar.check.png", UriKind.RelativeOrAbsolute)},
            {IconType.DELETE,new Uri("/Assets/Icons/appbar.delete.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK_LIST,new Uri("/Assets/Icons/appbar.list.check.png", UriKind.RelativeOrAbsolute)}
        };

        public static IDictionary<IconType, Uri> IconUri { get { return _IconUri; } }
    }


    public class Page1ViewModel : ReactiveObject, IMyVM
    {
        public const string URI = "Page1";
        private readonly IThreadIdFactory ThreadIdFactory;
        private readonly IMessageBus Bus;


        public string UrlPathSegment
        {
            get { return URI; }
        }

        public IScreen HostScreen { get; private set; }


        public Page1ViewModel(IScreen host, IThreadIdFactory threadIdFactory, IMessageBus bus, bool IsInDesignMode = false)
        {
            this.HostScreen = host;
            this.ThreadIdFactory = threadIdFactory;
            this.Bus = bus;
            this.Bus.Listen<IEvent>().Subscribe(e => this.Events.Add(e));

            if (IsInDesignMode)
            {
                this.Events.Add(new MyEvent(DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
            }

        }


        private ReactiveList<IAppBarButton> _AppBarButtons;
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


        private ObservableAsPropertyHelper<string> _WorkerResult;
        public string WorkerResult { get { return _WorkerResult.Value; } }

        private IObservable<bool> IsWorking = Observable.Return(true);

        private string _Status = "Idle";
        public string Status
        {
            get { return _Status; }
            set { this.RaiseAndSetIfChanged(ref _Status, value); }
        }


        private ReactiveList<IEvent> _Events;
        public ReactiveList<IEvent> Events
        {
            get
            {
                return _Events ?? (_Events = new ReactiveList<IEvent>());
            }
        }


        private ReactiveCommand _DoWork;

        public ReactiveCommand DoWork
        {
            get
            {
                if (_DoWork == null)
                {
                    _DoWork = new ReactiveCommand(IsWorking, false, null, true);
                    _DoWork.Subscribe(_ => this.Status = "Working");
                    _DoWork
                        .RegisterAsyncTask(this.Work)
                        .Do(_ => this.Status = "Idle")
                        .ToProperty(this, x => x.WorkerResult, out this._WorkerResult, ThreadIdFactory.CurrentId.ToString());
                    //_DoWork.AllowsConcurrentExecution = true;
                }
                return _DoWork;
            }
        }

        public Task<string> Work(object param)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(2000);
                this.Bus.SendMessage<IEvent>(new MyEvent(DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
                return ThreadIdFactory.CurrentId.ToString();
            });
        }


        public void SetupAppBarButtons()
        {
            this.AppBarButtons.Add(new ButtonViewModel()
            {
                Text = "Page2",
                Command = HostScreen.Router.NavigateCommandFor<Page2ViewModel>(),
                IconUri = Icons.IconUri[IconType.ADD]
            });
            this.AppBarButtons.Add(new ButtonViewModel()
            {
                Text = "Start",
                Command = this.DoWork,
                IconUri = Icons.IconUri[IconType.ADD]
            });
        }

        public void ClearAppBarButtons()
        {
            this.AppBarButtons.RemoveRange(0, this.AppBarButtons.Count);

        }




    }
}
