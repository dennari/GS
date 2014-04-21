
using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{
    public class TestingViewModel : GSViewModelBase
    {

        private string _ExceptionType = "normal";
        public string ExceptionType
        {
            get
            {
                return _ExceptionType;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ExceptionType, value);
            }
        }


        public TestingViewModel(IGSAppViewModel app)
            : base(app)
        {
            this.CreateLocalDataCommand = new ReactiveCommand(Observable.Return(true), false);
            CreateLocalDataCommand.IsExecuting.ToProperty(this, x => x.CreateLocalDataCommandIsExecuting, out _CreateLocalDataCommandIsExecuting, true);

            this.CreateRemoteDataCommand = new ReactiveCommand();
            this.PushRemoteUserCommand = new ReactiveCommand();
            this.ClearDBCommand = new ReactiveCommand();
            this.SyncCommand = new ReactiveCommand();
            this.PushCommand = new ReactiveCommand();
            this.ResetCommand = new ReactiveCommand();
            this.RegisterCommand = new ReactiveCommand();
            this.MultideleteAllCommand = new ReactiveCommand();
            this.RegisterCommand.Subscribe(_ => this.Navigate(new SignInRegisterViewModel(App)));

            this.ThrowExceptionCommand = new ReactiveCommand();

            this.ThrowExceptionCommand.Subscribe(_ =>
            {


                switch (ExceptionType)
                {
                    case "async":
                        Task.Run(() =>
                        {
                            //await Task.Delay(300);
                            throw new Exception("TestingViewModel, task exception");

                        });
                        break;
                    case "asyncvoid":
                        ThrowTaskVoidException();
                        break;
                    default:
                        throw new Exception("TestingViewModel, normal exception");

                }

            });

            ThrowExceptionCommand.ThrownExceptions
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(e =>
                {
                    throw e;
                });

        }

        protected async Task ThrowTaskException()
        {
            var task = Task.Run(() =>
            {
                //await Task.Delay(300);
                throw new Exception("TestingViewModel, task exception");

            });

            try
            {
                await task;
            }
            catch (Exception e)
            {
                throw e;
            }


        }

        protected async void ThrowTaskVoidException()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(300);
                throw new Exception("TestingViewModel, task void exception");

            });
        }

        protected ObservableAsPropertyHelper<bool> _CreateLocalDataCommandIsExecuting;
        public bool CreateLocalDataCommandIsExecuting
        {
            get { return _CreateLocalDataCommandIsExecuting.Value; }
        }

        public ReactiveCommand CreateLocalDataCommand { get; protected set; }
        public ReactiveCommand ThrowExceptionCommand { get; protected set; }
        public ReactiveCommand CreateRemoteDataCommand { get; protected set; }
        public ReactiveCommand PushRemoteUserCommand { get; protected set; }
        public ReactiveCommand ClearDBCommand { get; protected set; }
        public ReactiveCommand SyncCommand { get; protected set; }
        public ReactiveCommand PushCommand { get; protected set; }
        public ReactiveCommand RegisterCommand { get; protected set; }
        public ReactiveCommand ResetCommand { get; protected set; }
        public ReactiveCommand MultideleteAllCommand { get; protected set; }



    }
}
