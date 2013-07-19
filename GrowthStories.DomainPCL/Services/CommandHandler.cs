using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using EventStore.Logging;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Microsoft.CSharp.RuntimeBinder;
//using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Services
{
    public class NullCommandHandler : IDispatchCommands
    {

        public TEntity Handle<TEntity, TCommand>(TCommand c)
            where TEntity : class, ICommandHandler<TCommand>, IGSAggregate, new()
            where TCommand : IEntityCommand
        {
            return null;
        }

        public void HandlerHandle<TEntity, TCommand>(TCommand c)
            where TEntity : class, IGSAggregate, new()
            where TCommand : IEntityCommand
        {
            //throw new NotImplementedException();
        }

        public Task<object> HandleAsync<TEntity, TCommand>(TCommand c)
            where TEntity : class, IAsyncCommandHandler<TCommand>, IGSAggregate, new()
            where TCommand : IEntityCommand
        {
            return null;
        }

        public Task<object> HandlerHandleAsync<TEntity, TCommand>(TCommand c)
            where TEntity : class, IGSAggregate, new()
            where TCommand : IEntityCommand
        {
            return null;
        }
    }
}
