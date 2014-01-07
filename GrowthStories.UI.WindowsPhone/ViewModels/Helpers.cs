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

    /*
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
  
    */
}


//namespace Growthstories.Domain.Entities
//{
//    public sealed class PlantState
//    {
//        public string Name { get; set; }
//        public string Species { get; set; }
//    }
//}