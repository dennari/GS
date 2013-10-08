using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public interface IDispatchCommands
    {


        IGSAggregate Handle(IAggregateCommand c);

    }

    public interface IRegisterHandlers
    {
        IDictionary<Tuple<Type, Type>, Action<IGSAggregate, IAggregateCommand>> RegisterHandlers();
        IDictionary<Tuple<Type, Type>, Func<IGSAggregate, IAggregateCommand, Task<object>>> RegisterAsyncHandlers();

    }

    public interface IRegisterEventHandlers
    {
        void Register<TEvent>(IEventHandler<TEvent> handler)
            where TEvent : IEvent;

        void RegisterAsync<TEvent>(IAsyncEventHandler<TEvent> handler)
            where TEvent : IEvent;

    }

    public interface IRegisterCommandHandlers
    {
        void Register<TCommand>(ICommandHandler<TCommand> handler)
            where TCommand : ICommand;

        void RegisterAsync<TCommand>(IAsyncCommandHandler<TCommand> handler)
            where TCommand : ICommand;

    }

    public static class MBExtensions
    {
        public static void Handle<T>(this IMessageBus bus, T message)
        {
            bus.SendMessage(message);
        }

        public static void SendCommand<T>(this IMessageBus bus, T message)
            where T : IAggregateCommand
        {
            bus.SendMessage((IAggregateCommand)message);
        }
    }


}
