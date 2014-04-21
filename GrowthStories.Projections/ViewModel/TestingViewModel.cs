
using System;
using ReactiveUI;
using System.Reactive.Linq;

namespace Growthstories.UI.ViewModel
{
    public class TestingViewModel : GSViewModelBase
    {
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
        }

        protected ObservableAsPropertyHelper<bool> _CreateLocalDataCommandIsExecuting;
        public bool CreateLocalDataCommandIsExecuting
        {
            get { return _CreateLocalDataCommandIsExecuting.Value; }
        }

        public ReactiveCommand CreateLocalDataCommand { get; protected set; }
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
