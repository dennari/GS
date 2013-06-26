using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
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
    public class CommandHandler : IDispatchCommands
    {

        readonly IGSRepository _repository;
        readonly IAggregateFactory _factory;

        public CommandHandler(IGSRepository store, IAggregateFactory factory)
            : base()
        {
            _repository = store;
            _factory = factory;
        }


        //public void Handle(ICommand e)
        //{

        //    try
        //    {
        //        ((dynamic)this).When((dynamic)e);
        //    }
        //    catch (RuntimeBinderException ee)
        //    {
        //        throw;
        //    }
        //}


        public void Handle<TEntity, TCommand>(TCommand c)
            where TEntity : class, ICommandHandler<TCommand>, IGSAggregate, new()
            where TCommand : IEntityCommand
        {

            var u = _factory.Build<TEntity>();
            _repository.PlayById(u, c.EntityId);
            u.Handle(c);
            _repository.Save(u);

        }

    }
}
