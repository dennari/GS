using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Domain
{
    public interface IDispatchCommands
    {

        void Handle<TEntity, TCommand>(TCommand c)
            where TEntity : class, ICommandHandler<TCommand>, IGSAggregate, new()
            where TCommand : IEntityCommand;
    }
}
