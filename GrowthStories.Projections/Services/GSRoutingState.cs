using System;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;

namespace Growthstories.UI.Services
{

    public interface IGSRoutingState : IRoutingState
    {
        void Reset();
    }

    public class GSRoutingState : ReactiveObject, IGSRoutingState
    {

        ReactiveList<IRoutableViewModel> _NavigationStack;

        /// <summary>
        /// Represents the current navigation stack, the last element in the
        /// collection being the currently visible ViewModel.
        /// </summary>

        public ReactiveList<IRoutableViewModel> NavigationStack
        {
            get { return _NavigationStack; }
            protected set { _NavigationStack = value; }
        }

        /// <summary>
        /// Navigates back to the previous element in the stack.
        /// </summary>

        public IReactiveCommand NavigateBack { get; protected set; }

        /// <summary>
        /// Navigates to the a new element in the stack - the Execute parameter
        /// must be a ViewModel that implements IRoutableViewModel.
        /// </summary>

        public INavigateCommand Navigate { get; protected set; }

        /// <summary>
        /// Navigates to a new element and resets the navigation stack (i.e. the
        /// new ViewModel will now be the only element in the stack) - the
        /// Execute parameter must be a ViewModel that implements
        /// IRoutableViewModel.
        /// </summary>

        public INavigateCommand NavigateAndReset { get; protected set; }

        public void Reset()
        {
            NavigationStack.Clear();

        }



        public IObservable<IRoutableViewModel> CurrentViewModel { get; protected set; }

        public GSRoutingState()
        {
            _NavigationStack = new ReactiveList<IRoutableViewModel>();
            setupRx();
        }


        void setupRx()
        {
            NavigateBack = new ReactiveCommand(
                NavigationStack.CountChanged.StartWith(_NavigationStack.Count).Select(x => x > 0));
            NavigateBack.Subscribe(_ =>
                NavigationStack.RemoveAt(NavigationStack.Count - 1));

            Navigate = new NavigationReactiveCommand();
            Navigate.Subscribe(x =>
            {
                var vm = x as IRoutableViewModel;
                if (vm == null)
                {
                    throw new Exception("Navigate must be called on an IRoutableViewModel");
                }


                if (vm != NavigationStack.LastOrDefault())
                    NavigationStack.Add(vm);
            });

            NavigateAndReset = new NavigationReactiveCommand();
            NavigateAndReset.Subscribe(x =>
            {
                NavigationStack.Clear();
                Navigate.Execute(x);
            });

            CurrentViewModel = Observable.Concat(
                Observable.Defer(() => Observable.Return(NavigationStack.LastOrDefault())),
                NavigationStack.Changed.Select(_ => NavigationStack.LastOrDefault()));
        }
    }

    class NavigationReactiveCommand : ReactiveCommand, INavigateCommand { }

}
