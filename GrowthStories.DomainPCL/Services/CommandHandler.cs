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

        public readonly IDictionary<Type, IList<Guid>> OtherHandlers;

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
            OtherHandlers = new Dictionary<Type, IList<Guid>>();

        }



        protected TEntity Construct<TEntity>(IAggregateCommand c) where TEntity : class, IGSAggregate, new()
        {
            Logger.Info(c.ToString());
            var u = _factory.Build<TEntity>();
            _repository.PlayById(u, c.AggregateId);
            return u;
        }

        //public void Handle(ICommand e)



        public IGSAggregate Handle(IAggregateCommand c)
        {
            IGSAggregate aggregate = null;
            ICreateCommand cc = c as ICreateCommand;
            if (cc != null)
            {
                aggregate = (IGSAggregate)_factory.Build(cc.AggregateType);
                _repository.PlayById(aggregate, c.AggregateId);
            }

            if (aggregate == null)
            {
                aggregate = _repository.GetById(c.AggregateId);
            }

            var handlers = new List<IGSAggregate>() { aggregate };

            IList<Guid> OtherIds = null;
            if (OtherHandlers.TryGetValue(c.GetType(), out OtherIds))
                handlers.AddRange(OtherIds.Select(x => _repository.GetById(x)));
            Logger.Info("Handling command:" + c.ToString());


            foreach (var agg in handlers)
            {
                try
                {
                    ((dynamic)agg).Handle((dynamic)c);
                }
                catch
                {

                }
            }

            //var changes = aggregate.GetUncommittedEvents();
            _persistence.RunInTransaction(() => _repository.Save(aggregate));



            return aggregate;

        }
    }
}
