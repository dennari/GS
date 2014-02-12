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

namespace WeakEventSource
{
    /// <summary>
    /// Provides a non-generic base class for the WeakEventListener implementation.
    /// The main reason is to get easier reading code in the WeakEventSourceBase implementation. 
    /// </summary>
    public abstract class WeakEventListenerBase
    {
        public abstract void Detach();
    }
}
