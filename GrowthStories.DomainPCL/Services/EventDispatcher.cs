using EventStore;
using EventStore.Dispatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Growthstories.Core;
using System.Reflection;
using System.Threading.Tasks;
using Nito.AsyncEx;
using ReactiveUI;

namespace Growthstories.Domain.Services
{

    //public class Binding 
    //{
    //    Action<TEvent> Bind<TEvent>(TEvent @event)
    //    {
    //        return (TEvent e)
    //    } 
    //}

    public class EventDispatcher : IAsyncDispatchCommits, IRegisterEventHandlers
    {

        private IDictionary<Type, ISet<IEventHandler>> EventTypeToHandlers = new Dictionary<Type, ISet<IEventHandler>>();
        private IDictionary<Type, ISet<IAsyncEventHandler>> EventTypeToAsyncHandlers = new Dictionary<Type, ISet<IAsyncEventHandler>>();
        //private IList<Func<Task>> AsyncQueue = new List<Func<Task>>();
        private Queue<Func<Task>> AsyncQueue = new Queue<Func<Task>>();

        //private readonly MethodInfo _InvokeHandler;
        public EventDispatcher()
        {
            //_InvokeHandler = GetType().GetTypeInfo().GetDeclaredMethod("InvokeHandler");
        }

        public void Register<TEvent>(IEventHandler<TEvent> handler)
            where TEvent : IEvent
        {
            Type eType = typeof(TEvent);
            ISet<IEventHandler> handlers = null;
            if (!this.EventTypeToHandlers.TryGetValue(eType, out handlers))
                this.EventTypeToHandlers[eType] = handlers = new HashSet<IEventHandler>();
            handlers.Add(handler);
        }

        public void RegisterAsync<TEvent>(IAsyncEventHandler<TEvent> handler) where TEvent : IEvent
        {
            Type eType = typeof(TEvent);
            ISet<IAsyncEventHandler> handlers = null;
            if (!this.EventTypeToAsyncHandlers.TryGetValue(eType, out handlers))
                this.EventTypeToAsyncHandlers[eType] = handlers = new HashSet<IAsyncEventHandler>();
            handlers.Add(handler);
        }

        void InvokeHandler<TEvent>(IEventHandler<TEvent> handler, TEvent @event)
            where TEvent : IEvent
        {
            handler.Handle(@event);
        }

        public void Dispatch(Commit commit)
        {
            foreach (var e in commit.ActualEvents())
            {
                ISet<IEventHandler> handlers = null;
                //var eType = e.GetType();
                if (EventTypeToHandlers.TryGetValue(e.GetType(), out handlers))
                {
                    foreach (var h in handlers)
                    {
                        //_InvokeHandler.MakeGenericMethod(eType).Invoke(this, new[] { h, e });
                        h.Handle(e);
                    }
                }

                ISet<IAsyncEventHandler> asyncHandlers = null;
                //var eType = e.GetType();
                if (EventTypeToAsyncHandlers.TryGetValue(e.GetType(), out asyncHandlers))
                {
                    foreach (var h in asyncHandlers)
                    {
                        //_InvokeHandler.MakeGenericMethod(eType).Invoke(this, new[] { h, e });
                        //await h.HandleAsync(e);
                        this.AsyncQueue.Enqueue(async () => await h.HandleAsync(e));
                    }
                }

            }
        }

        private readonly AsyncLock m_lock = new AsyncLock();

        public Task DispatchAsync()
        {
            if (this.AsyncQueue.Count == 0)
                return null;
            return Task.Run(async () =>
            {
                //using (var lockk = await m_lock.LockAsync())
                //{
                //   while (AsyncQueue.Count > 0)
                // {
                var a = AsyncQueue.Dequeue();
                await a();
                //}
                //}

            });
        }




        public void Dispose()
        {
            //throw new NotImplementedException();
        }



    }



}
