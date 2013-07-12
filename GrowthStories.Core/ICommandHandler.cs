using CommonDomain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Growthstories.Core
{


    public interface ICommandHandler
    {
        void Handle(ICommand command);
    }

    public interface ICommandHandler<TCommand> : ICommandHandler
        where TCommand : ICommand
    {
        void Handle(TCommand command);
    }

    public interface IAsyncCommandHandler
    {
        Task HandleAsync(ICommand command);
    }

    public interface IAsyncCommandHandler<TCommand> : IAsyncCommandHandler
        where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);
    }

}
