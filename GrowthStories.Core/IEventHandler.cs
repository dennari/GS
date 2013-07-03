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

    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
    }



    public interface IAsyncCommandHandler<TCommand> where TCommand : ICommand
    {
        Task<object> HandleAsync(TCommand command);
    }

}
