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
    public class CommandHandler : IDispatchCommands
    {

        readonly IGSRepository _repository;
        readonly IAggregateFactory _factory;
        readonly IRegisterHandlers[] _handlerContainers;
        IDictionary<Tuple<Type, Type>, Action<IGSAggregate, IEntityCommand>> _handlers;
        IDictionary<Tuple<Type, Type>, Func<IGSAggregate, IEntityCommand, Task<object>>> _asyncHandlers;
        private readonly IPersistSyncStreams _persistence;

        private static ILog Logger = LogFactory.BuildLogger(typeof(CommandHandler));


        public CommandHandler(
            IGSRepository store,
            IAggregateFactory factory,
            IRegisterHandlers[] handlerContainers,
            IPersistSyncStreams persistence
            )
            : base()
        {
            _repository = store;
            _persistence = persistence;
            _factory = factory;
            _handlerContainers = handlerContainers;
        }

        private Action<IGSAggregate, IEntityCommand> GetHandler(Type TEntity, Type TCommand)
        {

            if (_handlers == null)
            {
                foreach (var container in this._handlerContainers)
                {
                    if (_handlers == null)
                        _handlers = container.RegisterHandlers();
                    else
                        _handlers.Concat(container.RegisterHandlers());
                }
            }

            Action<IGSAggregate, IEntityCommand> r = null;
            _handlers.TryGetValue(Tuple.Create(TEntity, TCommand), out r);
            return r;
        }

        private Func<IGSAggregate, IEntityCommand, Task<object>> GetAsyncHandler(Type TEntity, Type TCommand)
        {
            if (_asyncHandlers == null)
            {
                foreach (var container in this._handlerContainers)
                {
                    IDictionary<Tuple<Type, Type>, Func<IGSAggregate, IEntityCommand, Task<object>>> handlers = container.RegisterAsyncHandlers();
                    if (_asyncHandlers == null)
                        _asyncHandlers = handlers;
                    else
                        _asyncHandlers.Concat(handlers);
                }
                if (_asyncHandlers == null)
                    throw new InvalidOperationException(string.Format("No async handler registered for {0},{1}", TEntity.Name, TCommand.Name));
            }

            Func<IGSAggregate, IEntityCommand, Task<object>> r = null;
            _asyncHandlers.TryGetValue(Tuple.Create(TEntity, TCommand), out r);
            return r;
        }

        protected TEntity Construct<TEntity>(IEntityCommand c) where TEntity : class, IGSAggregate, new()
        {
            Logger.Info(c.ToString());
            var u = _factory.Build<TEntity>();
            _repository.PlayById(u, c.EntityId);
            return u;
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


        public TEntity Handle<TEntity, TCommand>(TCommand c)
            where TEntity : class, ICommandHandler<TCommand>, IGSAggregate, new()
            where TCommand : IEntityCommand
        {

            var u = Construct<TEntity>(c);

            //_persistence.
            u.Handle(c);
            _persistence.RunInTransaction(() => _repository.Save(u));
            return u;

        }


        public IGSAggregate Handle(IEntityCommand c)
        {
            IGSAggregate aggregate = null;
            ICreateCommand cc = c as ICreateCommand;
            if (cc != null)
            {
                aggregate = (IGSAggregate)_factory.Build(cc.EntityType);
                _repository.PlayById(aggregate, c.EntityId);
            }
            else
                aggregate = _repository.GetById(c.EntityId);


            ((dynamic)aggregate).Handle((dynamic)c);

            _persistence.RunInTransaction(() => _repository.Save(aggregate));
            return aggregate;

        }

        public Task<IGSAggregate> HandleAsync(IEntityCommand c)
        {
            IGSAggregate aggregate = null;
            ICreateCommand cc = c as ICreateCommand;
            if (cc != null)
            {
                aggregate = (IGSAggregate)_factory.Build(cc.EntityType);
                _repository.PlayById(aggregate, c.EntityId);
            }
            else
                aggregate = _repository.GetById(c.EntityId);

            return Task.Run<IGSAggregate>(() =>
            {
                ((dynamic)aggregate).Handle((dynamic)c);

                _persistence.RunInTransaction(() => _repository.Save(aggregate));
                return aggregate;
            });

        }



        public void HandlerHandle<TEntity, TCommand>(TCommand c)
            where TEntity : class, IGSAggregate, new()
            where TCommand : IEntityCommand
        {
            var u = Construct<TEntity>(c);

            var handler = GetHandler(typeof(TEntity), typeof(TCommand));
            if (handler == null)
                throw new InvalidOperationException();

            _persistence.RunInTransaction(() =>
            {
                handler(u, c);
                _repository.Save(u);
            });

        }



        public Task HandleAsync<TEntity, TCommand>(TCommand c)
            where TEntity : class, IGSAggregate, IAsyncCommandHandler<TCommand>, new()
            where TCommand : IEntityCommand
        {

            var u = Construct<TEntity>(c);

            return Task.Run(async () =>
            {
                await u.HandleAsync(c);
                _persistence.RunInTransaction(() => _repository.Save(u));
                //return o;
            });


        }

        public Task<object> HandlerHandleAsync<TEntity, TCommand>(TCommand c)
            where TEntity : class, IGSAggregate, new()
            where TCommand : IEntityCommand
        {

            var u = Construct<TEntity>(c);


            var handler = GetAsyncHandler(typeof(TEntity), typeof(TCommand));
            if (handler == null)
                throw new InvalidOperationException();
            return Task.Run<object>(async () =>
            {

                object o = await handler(u, c);
                _persistence.RunInTransaction(() =>
               {
                   _repository.Save(u);
               });
                return o;

            });



        }



        Task<object> IDispatchCommands.HandleAsync<TEntity, TCommand>(TCommand c)
        {
            throw new NotImplementedException();
        }
    }
}
