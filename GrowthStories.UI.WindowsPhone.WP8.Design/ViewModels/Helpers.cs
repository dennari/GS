using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Collections;
using System.ComponentModel;
using System.Reactive;
using System.Collections.Specialized;

namespace ReactiveUI
{


    public class MockReactiveList<T> : List<T>, IReactiveList<T>, IReadOnlyReactiveList<T>
    {

        public MockReactiveList() : base() { }

        public MockReactiveList(IEnumerable<T> items) : base(items) { }

        public IObservable<object> ItemsAdded
        {
            get { return Observable.Return(new object()); }
        }

        public IObservable<object> BeforeItemsAdded
        {
            get { return Observable.Return(new object()); }
        }

        public IObservable<object> ItemsRemoved
        {
            get { return Observable.Return(new object()); }
        }

        public IObservable<object> BeforeItemsRemoved
        {
            get { return Observable.Return(new object()); }
        }

        public IObservable<IMoveInfo<object>> BeforeItemsMoved
        {
            get { throw new NotImplementedException(); }
        }

        public IObservable<IMoveInfo<object>> ItemsMoved
        {
            get { throw new NotImplementedException(); }
        }

        public IObservable<NotifyCollectionChangedEventArgs> Changing
        {
            get { throw new NotImplementedException(); }
        }

        public IObservable<NotifyCollectionChangedEventArgs> Changed
        {
            get { throw new NotImplementedException(); }
        }

        public IObservable<Unit> ShouldReset
        {
            get { throw new NotImplementedException(); }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IObservable<IObservedChange<object, object>> ItemChanging
        {
            get { throw new NotImplementedException(); }
        }

        public IObservable<IObservedChange<object, object>> ItemChanged
        {
            get { throw new NotImplementedException(); }
        }

        public bool ChangeTrackingEnabled
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        IObservable<T> IReactiveNotifyCollectionChanged<T>.ItemsAdded
        {
            get { return Observable.Return(default(T)); }
        }

        IObservable<T> IReactiveNotifyCollectionChanged<T>.BeforeItemsAdded
        {
            get { return Observable.Return(default(T)); }
        }

        IObservable<T> IReactiveNotifyCollectionChanged<T>.ItemsRemoved
        {
            get { return Observable.Return(default(T)); }
        }

        IObservable<T> IReactiveNotifyCollectionChanged<T>.BeforeItemsRemoved
        {
            get { return Observable.Return(default(T)); }
        }

        IObservable<IMoveInfo<T>> IReactiveNotifyCollectionChanged<T>.BeforeItemsMoved
        {
            get { throw new NotImplementedException(); }
        }

        IObservable<IMoveInfo<T>> IReactiveNotifyCollectionChanged<T>.ItemsMoved
        {
            get { throw new NotImplementedException(); }
        }

        IObservable<IObservedChange<T, object>> IReactiveNotifyCollectionItemChanged<T>.ItemChanging
        {
            get { throw new NotImplementedException(); }
        }

        IObservable<IObservedChange<T, object>> IReactiveNotifyCollectionItemChanged<T>.ItemChanged
        {
            get { throw new NotImplementedException(); }
        }
    }

    public interface IRoutableViewModel
    {

    }


    public interface IViewFor
    {
        object ViewModel { get; set; }
    }

    /// <summary>
    /// Implement this interface on your Views to support Routing and Binding.
    /// </summary>
    public interface IViewFor<T> : IViewFor
        where T : class
    {
        /// <summary>
        /// The ViewModel corresponding to this specific View. This should be
        /// a DependencyProperty if you're using XAML.
        /// </summary>
        new T ViewModel { get; set; }
    }

    public interface IScreen
    {
        /// <summary>
        /// The Router associated with this Screen.
        /// </summary>
        IRoutingState Router { get; }
    }

    public interface IRoutingState : IReactiveNotifyPropertyChanged
    {
        /// <summary>
        /// Represents the current navigation stack, the last element in the
        /// collection being the currently visible ViewModel.
        /// </summary>
        IReactiveList<IRoutableViewModel> NavigationStack { get; }

        /// <summary>
        /// Navigates back to the previous element in the stack.
        /// </summary>
        IReactiveCommand NavigateBack { get; }

        /// <summary>
        /// Navigates to the a new element in the stack - the Execute parameter
        /// must be a ViewModel that implements IRoutableViewModel.
        /// </summary>
        IReactiveCommand Navigate { get; }

        /// <summary>
        /// Navigates to a new element and resets the navigation stack (i.e. the
        /// new ViewModel will now be the only element in the stack) - the
        /// Execute parameter must be a ViewModel that implements
        /// IRoutableViewModel.
        /// </summary>
        IReactiveCommand NavigateAndReset { get; }

        IObservable<IRoutableViewModel> CurrentViewModel { get; }
    }

    public interface IMessageBus : IEnableLogger
    {
        /// <summary>
        /// Registers a scheduler for the type, which may be specified at
        /// runtime, and the contract.
        /// </summary>
        /// <remarks>If a scheduler is already registered for the specified
        /// runtime and contract, this will overrwrite the existing
        /// registration.</remarks>
        /// <typeparam name="T">The type of the message to listen to.</typeparam>
        /// <param name="scheduler">The scheduler on which to post the
        /// notifications for the specified type and contract.
        /// RxApp.MainThreadScheduler by default.</param>
        /// <param name="contract">A unique string to distinguish messages with
        /// identical types (i.e. "MyCoolViewModel") - if the message type is
        /// only used for one purpose, leave this as null.</param>
        //void RegisterScheduler<T>(IScheduler scheduler, string contract = null);

        /// <summary>
        /// Listen provides an Observable that will fire whenever a Message is
        /// provided for this object via RegisterMessageSource or SendMessage.
        /// </summary>
        /// <typeparam name="T">The type of the message to listen to.</typeparam>
        /// <param name="contract">A unique string to distinguish messages with
        /// identical types (i.e. "MyCoolViewModel") - if the message type is
        /// only used for one purpose, leave this as null.</param>
        /// <returns></returns>
        IObservable<T> Listen<T>(string contract = null);

        /// <summary>
        /// ListenIncludeLatest provides an Observable that will fire whenever a Message is
        /// provided for this object via RegisterMessageSource or SendMessage and fire the 
        /// last provided Message immediately if applicable, or null.
        /// </summary>
        /// <typeparam name="T">The type of the message to listen to.</typeparam>
        /// <param name="contract">A unique string to distinguish messages with
        /// identical types (i.e. "MyCoolViewModel") - if the message type is
        /// only used for one purpose, leave this as null.</param>
        /// <returns>An Observable representing the notifications posted to the
        /// message bus.</returns>
        IObservable<T> ListenIncludeLatest<T>(string contract = null);

        /// <summary>
        /// Determines if a particular message Type is registered.
        /// </summary>
        /// <param name="type">The type of the message.</param>
        /// <param name="contract">A unique string to distinguish messages with
        /// identical types (i.e. "MyCoolViewModel") - if the message type is
        /// only used for one purpose, leave this as null.</param>
        /// <returns>True if messages have been posted for this message Type.</returns>
        bool IsRegistered(Type type, string contract = null);

        /// <summary>
        /// Registers an Observable representing the stream of messages to send.
        /// Another part of the code can then call Listen to retrieve this
        /// Observable.
        /// </summary>
        /// <typeparam name="T">The type of the message to listen to.</typeparam>
        /// <param name="source">An Observable that will be subscribed to, and a
        /// message sent out for each value provided.</param>
        /// <param name="contract">A unique string to distinguish messages with
        /// identical types (i.e. "MyCoolViewModel") - if the message type is
        /// only used for one purpose, leave this as null.</param>
        IDisposable RegisterMessageSource<T>(IObservable<T> source, string contract = null);

        /// <summary>
        /// Sends a single message using the specified Type and contract.
        /// Consider using RegisterMessageSource instead if you will be sending
        /// messages in response to other changes such as property changes
        /// or events.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The actual message to send</param>
        /// <param name="contract">A unique string to distinguish messages with
        /// identical types (i.e. "MyCoolViewModel") - if the message type is
        /// only used for one purpose, leave this as null.</param>
        void SendMessage<T>(T message, string contract = null);
    }

    public interface IDependencyResolver : IDisposable
    {
        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>. Must return <c>null</c>
        /// if the service is not available (must not throw).
        /// </summary>
        /// <param name="serviceType">The object type.</param>
        /// <returns>The requested object, if found; <c>null</c> otherwise.</returns>
        object GetService(Type serviceType, string contract = null);

        /// <summary>
        /// Gets all instances of the given <paramref name="serviceType"/>. Must return an empty
        /// collection if the service is not available (must not return <c>null</c> or throw).
        /// </summary>
        /// <param name="serviceType">The object type.</param>
        /// <returns>A sequence of instances of the requested <paramref name="serviceType"/>. The sequence
        /// should be empty (not <c>null</c>) if no objects of the given type are available.</returns>
        IEnumerable<object> GetServices(Type serviceType, string contract = null);
    }

    /// <summary>
    /// Represents a dependency resolver where types can be registered after 
    /// setup.
    /// </summary>
    public interface IMutableDependencyResolver : IDependencyResolver
    {
        void Register(Func<object> factory, Type serviceType, string contract = null);
    }


    public class MockReactiveCommand : IReactiveCommand
    {

        private readonly Action<object> Aktion;


        public MockReactiveCommand() { }
        public MockReactiveCommand(Action<object> action)
        {
            this.Aktion = action;
        }

        public IObservable<T> RegisterAsync<T>(Func<object, IObservable<T>> asyncBlock)
        {
            throw new NotImplementedException();
        }

        public IObservable<bool> CanExecuteObservable
        {
            get { return Observable.Return(false); }
        }

        public IObservable<bool> IsExecuting
        {
            get { return Observable.Return(false); }
        }

        public bool AllowsConcurrentExecution
        {
            get { return false; }

        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            //throw new NotImplementedException();
            return Disposable.Empty;
        }

        public bool CanExecute(object parameter)
        {
            //throw new NotImplementedException();
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (Aktion != null)
                Aktion(parameter);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public IObservable<Exception> ThrownExceptions
        {
            get { return null; }
        }
    }

    public static class RoutableViewModelMixin
    {
        /// <summary>
        /// This method allows you to set up connections that only operate
        /// while the ViewModel has focus, and cleans up when the ViewModel
        /// loses focus.
        /// </summary>
        /// <param name="onNavigatedTo">Called when the ViewModel is navigated
        /// to - return an IDisposable that cleans up all of the things that are
        /// configured in the method.</param>
        /// <returns>An IDisposable that lets you disconnect the entire process
        /// earlier than normal.</returns>
        public static IDisposable WhenNavigatedTo(this IRoutableViewModel This, Func<IDisposable> onNavigatedTo)
        {
            return Disposable.Empty;

        }

        public static IDisposable WhenNavigatedTo<TView, TViewModel>(this TView This, TViewModel viewModel, Func<IDisposable> onNavigatedTo)
            where TView : IViewFor<TViewModel>
            where TViewModel : class, IRoutableViewModel
        {
            return Disposable.Empty;

        }

    }

    /// </summary>
    public interface IObservedChange<out TSender, out TValue>
    {
        /// <summary>
        /// The object that has raised the change.
        /// </summary>
        TSender Sender { get; }

        /// <summary>
        /// The name of the property that has changed on Sender.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// The value of the property that has changed. IMPORTANT NOTE: This
        /// property is often not set for performance reasons, unless you have
        /// explicitly requested an Observable for a property via a method such
        /// as ObservableForProperty. To retrieve the value for the property,
        /// use the Value() extension method.
        /// </summary>
        TValue Value { get; }
    }

    /// <summary>
    /// A data-only version of IObservedChange
    /// </summary>
    public class ObservedChange<TSender, TValue> : IObservedChange<TSender, TValue>
    {
        public TSender Sender { get; set; }
        public string PropertyName { get; set; }
        public TValue Value { get; set; }
    }

    /// <summary>
    /// IReactiveNotifyPropertyChanged represents an extended version of
    /// INotifyPropertyChanged that also exposes Observables.
    /// </summary>
    public interface IReactiveNotifyPropertyChanged : INotifyPropertyChanged, INotifyPropertyChanging, IEnableLogger
    {
        /// <summary>
        /// Represents an Observable that fires *before* a property is about to
        /// be changed. Note that this should not fire duplicate change notifications if a
        /// property is set to the same value multiple times.
        /// </summary>
        IObservable<IObservedChange<object, object>> Changing { get; }

        /// <summary>
        /// Represents an Observable that fires *after* a property has changed.
        /// Note that this should not fire duplicate change notifications if a
        /// property is set to the same value multiple times.
        /// </summary>
        IObservable<IObservedChange<object, object>> Changed { get; }

        /// <summary>
        /// When this method is called, an object will not fire change
        /// notifications (neither traditional nor Observable notifications)
        /// until the return value is disposed.
        /// </summary>
        /// <returns>An object that, when disposed, reenables change
        /// notifications.</returns>
        IDisposable SuppressChangeNotifications();
    }

    /// <summary>
    /// IReactiveNotifyPropertyChanged of TSender is a helper interface that adds
    /// typed versions of Changing and Changed.
    /// </summary>
    public interface IReactiveNotifyPropertyChanged<out TSender> : IReactiveNotifyPropertyChanged
    {
        new IObservable<IObservedChange<TSender, object>> Changing { get; }
        new IObservable<IObservedChange<TSender, object>> Changed { get; }
    }

    /// <summary>
    /// This interface is implemented by RxUI objects which are given 
    /// IObservables as input - when the input IObservables OnError, instead of 
    /// disabling the RxUI object, we catch the IObservable and pipe it into
    /// this property.
    /// 
    /// Normally this IObservable is implemented with a ScheduledSubject whose 
    /// default Observer is RxApp.DefaultExceptionHandler - this means, that if
    /// you aren't listening to ThrownExceptions and one appears, the exception
    /// will appear on the UI thread and crash the application.
    /// </summary>
    public interface IHandleObservableErrors
    {
        /// <summary>
        /// Fires whenever an exception would normally terminate ReactiveUI 
        /// internal state.
        /// </summary>
        IObservable<Exception> ThrownExceptions { get; }
    }

    /// <summary>
    /// IReactiveCommand represents an ICommand which also notifies when it is
    /// executed (i.e. when Execute is called) via IObservable. Conceptually,
    /// this represents an Event, so as a result this IObservable should never
    /// OnComplete or OnError.
    /// 
    /// In previous versions of ReactiveUI, this interface was split into two
    /// separate interfaces, one to handle async methods and one for "standard"
    /// commands, but these have now been merged - every ReactiveCommand is now
    /// a ReactiveAsyncCommand.
    /// </summary>
    public interface IReactiveCommand : IHandleObservableErrors, IObservable<object>, ICommand, IDisposable, IEnableLogger
    {
        /// <summary>
        /// Registers an asynchronous method to be called whenever the command
        /// is Executed. This method returns an IObservable representing the
        /// asynchronous operation, and is allowed to OnError / should OnComplete.
        /// </summary>
        /// <returns>A filtered version of the Observable which is marshaled 
        /// to the UI thread. This Observable should only report successes and
        /// instead send OnError messages to the ThrownExceptions property.
        /// </returns>
        /// <param name="asyncBlock">The asynchronous method to call.</param>
        IObservable<T> RegisterAsync<T>(Func<object, IObservable<T>> asyncBlock);

        /// <summary>
        /// Gets a value indicating whether this instance can execute observable.
        /// </summary>
        /// <value><c>true</c> if this instance can execute observable; otherwise, <c>false</c>.</value>
        IObservable<bool> CanExecuteObservable { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is executing. This 
        /// Observable is guaranteed to always return a value immediately (i.e.
        /// it is backed by a BehaviorSubject), meaning it is safe to determine
        /// the current state of the command via IsExecuting.First()
        /// </summary>
        /// <value><c>true</c> if this instance is executing; otherwise, <c>false</c>.</value>
        IObservable<bool> IsExecuting { get; }

        /// <summary>
        /// Gets a value indicating whether this 
        /// <see cref="ReactiveUI.IReactiveCommand"/> allows concurrent 
        /// execution. If false, the CanExecute of the command will be disabled
        /// while async operations are currently in-flight.
        /// </summary>
        /// <value><c>true</c> if allows concurrent execution; otherwise, <c>false</c>.</value>
        bool AllowsConcurrentExecution { get; }
    }

    /// <summary>
    /// IReactiveCollection represents a collection that can notify when its
    /// contents are changed (either items are added/removed, or the object
    /// itself changes).
    ///
    /// It is important to implement the Changing/Changed from
    /// IReactiveNotifyPropertyChanged semantically as "Fire when *anything* in
    /// the collection or any of its items have changed, in any way".
    /// </summary>
    public interface IReactiveCollection : IReactiveNotifyCollectionChanged, IReactiveNotifyCollectionItemChanged, ICollection, INotifyPropertyChanging, INotifyPropertyChanged, IEnableLogger
    {
    }

    /// <summary>
    /// IReactiveCollection of T is the typed version of IReactiveCollection and
    /// adds type-specified versions of Observables
    /// </summary>
    public interface IReactiveCollection<T> : IReactiveCollection, ICollection<T>, IReactiveNotifyCollectionChanged<T>, IReactiveNotifyCollectionItemChanged<T>
    {
    }

    /// <summary>
    /// IReactiveNotifyCollectionItemChanged provides notifications for collection item updates, ie when an object in
    /// a collection changes.
    /// </summary>
    public interface IReactiveNotifyCollectionItemChanged
    {
        /// <summary>
        /// Provides Item Changing notifications for any item in collection that
        /// implements IReactiveNotifyPropertyChanged. This is only enabled when
        /// ChangeTrackingEnabled is set to True.
        /// </summary>
        IObservable<IObservedChange<object, object>> ItemChanging { get; }

        /// <summary>
        /// Provides Item Changed notifications for any item in collection that
        /// implements IReactiveNotifyPropertyChanged. This is only enabled when
        /// ChangeTrackingEnabled is set to True.
        /// </summary>
        IObservable<IObservedChange<object, object>> ItemChanged { get; }

        /// <summary>
        /// Enables the ItemChanging and ItemChanged properties; when this is
        /// enabled, whenever a property on any object implementing
        /// IReactiveNotifyPropertyChanged changes, the change will be
        /// rebroadcast through ItemChanging/ItemChanged.
        /// </summary>
        bool ChangeTrackingEnabled { get; set; }
    }

    /// <summary>
    /// IReactiveNotifyCollectionItemChanged of T is the typed version of IReactiveNotifyCollectionItemChanged and
    /// adds type-specified versions of Observables
    /// </summary>
    public interface IReactiveNotifyCollectionItemChanged<out T> : IReactiveNotifyCollectionItemChanged
    {
        /// <summary>
        /// Provides Item Changing notifications for any item in collection that
        /// implements IReactiveNotifyPropertyChanged. This is only enabled when
        /// ChangeTrackingEnabled is set to True.
        /// </summary>
        new IObservable<IObservedChange<T, object>> ItemChanging { get; }

        /// <summary>
        /// Provides Item Changed notifications for any item in collection that
        /// implements IReactiveNotifyPropertyChanged. This is only enabled when
        /// ChangeTrackingEnabled is set to True.
        /// </summary>
        new IObservable<IObservedChange<T, object>> ItemChanged { get; }
    }

    /// <summary>
    /// IReactiveCollection provides notifications when the contents
    /// of collection are changed (items are added/removed/moved).
    /// </summary>
    public interface IReactiveNotifyCollectionChanged : INotifyCollectionChanged
    {
        /// <summary>
        /// Fires when items are added to the collection, once per item added.
        /// Functions that add multiple items such AddRange should fire this
        /// multiple times. The object provided is the item that was added.
        /// </summary>
        IObservable<object> ItemsAdded { get; }

        /// <summary>
        /// Fires before an item is going to be added to the collection.
        /// </summary>
        IObservable<object> BeforeItemsAdded { get; }

        /// <summary>
        /// Fires once an item has been removed from a collection, providing the
        /// item that was removed.
        /// </summary>
        IObservable<object> ItemsRemoved { get; }

        /// <summary>
        /// Fires before an item will be removed from a collection, providing
        /// the item that will be removed. 
        /// </summary>
        IObservable<object> BeforeItemsRemoved { get; }

        /// <summary>
        /// Fires before an items moves from one position in the collection to
        /// another, providing the item(s) to be moved as well as source and destination
        /// indices.
        /// </summary>
        IObservable<IMoveInfo<object>> BeforeItemsMoved { get; }

        /// <summary>
        /// Fires once one or more items moves from one position in the collection to
        /// another, providing the item(s) that was moved as well as source and destination
        /// indices.
        /// </summary>
        IObservable<IMoveInfo<object>> ItemsMoved { get; }

        /// <summary>
        /// This Observable is equivalent to the NotifyCollectionChanged event,
        /// but fires before the collection is changed
        /// </summary>
        IObservable<NotifyCollectionChangedEventArgs> Changing { get; }

        /// <summary>
        /// This Observable is equivalent to the NotifyCollectionChanged event,
        /// and fires after the collection is changed
        /// </summary>
        IObservable<NotifyCollectionChangedEventArgs> Changed { get; }

        /// <summary>
        /// This Observable is fired when a ShouldReset fires on the collection. This
        /// means that you should forget your previous knowledge of the state
        /// of the collection and reread it.
        /// 
        /// This does *not* mean Clear, and if you interpret it as such, you are
        /// Doing It Wrong.
        /// </summary>
        IObservable<Unit> ShouldReset { get; }
    }

    /// <summary>
    /// IReactiveNotifyCollectionChanged of T is the typed version of IReactiveNotifyCollectionChanged and
    /// adds type-specified versions of Observables
    /// </summary>
    public interface IReactiveNotifyCollectionChanged<out T> : IReactiveNotifyCollectionChanged
    {
        /// <summary>
        /// Fires when items are added to the collection, once per item added.
        /// Functions that add multiple items such AddRange should fire this
        /// multiple times. The object provided is the item that was added.
        /// </summary>
        new IObservable<T> ItemsAdded { get; }

        /// <summary>
        /// Fires before an item is going to be added to the collection.
        /// </summary>
        new IObservable<T> BeforeItemsAdded { get; }

        /// <summary>
        /// Fires once an item has been removed from a collection, providing the
        /// item that was removed.
        /// </summary>
        new IObservable<T> ItemsRemoved { get; }

        /// <summary>
        /// Fires before an item will be removed from a collection, providing
        /// the item that will be removed. 
        /// </summary>
        new IObservable<T> BeforeItemsRemoved { get; }

        /// <summary>
        /// Fires before an items moves from one position in the collection to
        /// another, providing the item(s) to be moved as well as source and destination
        /// indices.
        /// </summary>
        new IObservable<IMoveInfo<T>> BeforeItemsMoved { get; }

        /// <summary>
        /// Fires once one or more items moves from one position in the collection to
        /// another, providing the item(s) that was moved as well as source and destination
        /// indices.
        /// </summary>
        new IObservable<IMoveInfo<T>> ItemsMoved { get; }
    }

    /// <summary>
    /// IReadOnlyReactiveCollection represents a read-only collection that can notify when its
    /// contents are changed (either items are added/removed, or the object
    /// itself changes).
    ///
    /// It is important to implement the Changing/Changed from
    /// IReactiveNotifyPropertyChanged semantically as "Fire when *anything* in
    /// the collection or any of its items have changed, in any way".
    /// </summary>
    public interface IReadOnlyReactiveCollection<out T> : IReadOnlyCollection<T>, IReactiveNotifyCollectionChanged<T>, IReactiveNotifyCollectionItemChanged<T>, INotifyPropertyChanging, INotifyPropertyChanged, IEnableLogger
    {
    }

    /// <summary>
    /// IReactiveList represents a list that can notify when its
    /// contents are changed (either items are added/removed, or the object
    /// itself changes).
    ///
    /// It is important to implement the Changing/Changed from
    /// IReactiveNotifyPropertyChanged semantically as "Fire when *anything* in
    /// the collection or any of its items have changed, in any way".
    /// </summary>
    public interface IReactiveList<T> : IReactiveCollection<T>, IList<T>, IList
    {
    }


    /// <summary>
    /// IReactiveList represents a read-only list that can notify when its
    /// contents are changed (either items are added/removed, or the object
    /// itself changes).
    ///
    /// It is important to implement the Changing/Changed from
    /// IReactiveNotifyPropertyChanged semantically as "Fire when *anything* in
    /// the collection or any of its items have changed, in any way".
    /// </summary>
    public interface IReadOnlyReactiveList<out T> : IReadOnlyReactiveCollection<T>, IReadOnlyList<T>
    {
    }

    /// <summary>
    /// "Implement" this interface in your class to get access to the Log() 
    /// Mixin, which will give you a Logger that includes the class name in the
    /// log.
    /// </summary>
    public interface IEnableLogger { }

    public interface IMoveInfo<out T>
    {
        IEnumerable<T> MovedItems { get; }
        int From { get; }
        int To { get; }
    }
}


//namespace Growthstories.Domain.Entities
//{
//    public sealed class PlantState
//    {
//        public string Name { get; set; }
//        public string Species { get; set; }
//    }
//}