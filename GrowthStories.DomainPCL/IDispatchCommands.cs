using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public interface IDispatchCommands
    {

        TEntity Handle<TEntity, TCommand>(TCommand c)
            where TEntity : class, ICommandHandler<TCommand>, IGSAggregate, new()
            where TCommand : IEntityCommand;

        IGSAggregate Handle(IEntityCommand c);

        void HandlerHandle<TEntity, TCommand>(TCommand c)
            where TEntity : class, IGSAggregate, new()
            where TCommand : IEntityCommand;

        Task<object> HandleAsync<TEntity, TCommand>(TCommand c)
            where TEntity : class,  IAsyncCommandHandler<TCommand>, IGSAggregate, new()
            where TCommand : IEntityCommand;

        Task<object> HandlerHandleAsync<TEntity, TCommand>(TCommand c)
            where TEntity : class, IGSAggregate, new()
            where TCommand : IEntityCommand;
    }

    public interface IRegisterHandlers
    {
        IDictionary<Tuple<Type, Type>, Action<IGSAggregate, IEntityCommand>> RegisterHandlers();
        IDictionary<Tuple<Type, Type>, Func<IGSAggregate, IEntityCommand, Task<object>>> RegisterAsyncHandlers();

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


}
