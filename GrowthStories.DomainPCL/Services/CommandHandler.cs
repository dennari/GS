using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using EventStore.Logging;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Microsoft.CSharp.RuntimeBinder;
using ReactiveUI;
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
        private readonly IPersistSyncStreams _persistence;
        private static ILog Logger = LogFactory.BuildLogger(typeof(CommandHandler));
        //private readonly IMessageBus _bus;


        public CommandHandler(
            IGSRepository store,
            IAggregateFactory factory,
            IPersistSyncStreams persistence
            )
            : base()
        {
            _repository = store;
            _persistence = persistence;
            _factory = factory;

        }



        protected TEntity Construct<TEntity>(IEntityCommand c) where TEntity : class, IGSAggregate, new()
        {
            Logger.Info(c.ToString());
            var u = _factory.Build<TEntity>();
            _repository.PlayById(u, c.EntityId);
            return u;
        }

        //public void Handle(ICommand e)



        public IGSAggregate Handle(IEntityCommand c)
        {
            IGSAggregate aggregate = null;
            ICreateCommand cc = c as ICreateCommand;
            if (cc != null)
            {
                aggregate = (IGSAggregate)_factory.Build(cc.EntityType);
                _repository.PlayById(aggregate, c.EntityId);
            }
            if (aggregate == null)
            {
                if (c.ParentId.HasValue)
                {
                    aggregate = _repository.GetById(c.ParentId.Value);
                }
            }
            if (aggregate == null)
            {
                aggregate = _repository.GetById(c.EntityId);
            }


            ((dynamic)aggregate).Handle((dynamic)c);

            //var changes = aggregate.GetUncommittedEvents();
            _persistence.RunInTransaction(() => _repository.Save(aggregate));



            return aggregate;

        }
    }
}
