using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Specialized;

namespace WeakEventSource
{
    /// <summary>
    /// Default CollectionChanged weak-event wrapper for INotifyCollectionChanged event source.
    /// </summary>
    public class CollectionChangedWeakEventSource : WeakEventSourceBase<INotifyCollectionChanged>
    {
        protected override WeakEventListenerBase CreateWeakEventListener(INotifyCollectionChanged eventObject)
        {
            var weakListener = new WeakEventListener<CollectionChangedWeakEventSource,
                                                     INotifyCollectionChanged,
                                                     NotifyCollectionChangedEventArgs>(this, eventObject);
            weakListener.OnDetachAction = (listener, source) =>
            {
                source.CollectionChanged -= listener.OnEvent;
            };
            weakListener.OnEventAction = (instance, source, e) =>
            {
                // fire event
                if (instance.CollectionChanged != null)
                    instance.CollectionChanged(source, e);
            };
            eventObject.CollectionChanged += weakListener.OnEvent;

            return weakListener;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

    }

}
