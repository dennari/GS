using CommonDomain.Persistence;
using CommonDomain.Persistence.EventStore;
using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Ninject;
using Ninject.Parameters;
using NUnit.Framework;
using SimpleTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonDomain;
//using CommonDomain.Core;
using Growthstories.Domain.Services;
using Ninject.Activation;
using Newtonsoft.Json;
using Growthstories.Sync;
using CommonDomain.Core;
using System.Reflection;
using ReactiveUI;

namespace Growthstories.DomainTests
{
    public class gs_spec : spec_syntax
    {
        IKernel kernel;


        protected Guid id = Guid.NewGuid();

        protected override Guid Id
        {
            get
            {
                return id;
            }
        }


        protected override void SetupServices()
        {
            if (kernel != null)
                kernel.Dispose();
            kernel = new StandardKernel(new TestModule());
        }

        protected override IStoreEvents GetEventStore()
        {
            return Get<IStoreEvents>();
        }


        public T Get<T>() { return kernel.Get<T>(); }
        public IMessageBus Handler { get { return Get<IMessageBus>(); } }
        public SynchronizerService Synchronizer { get { return Get<SynchronizerService>(); } }
        public string toJSON(object o) { return Get<IJsonFactory>().Serialize(o); }
        public IRepository Repository { get { return Get<IRepository>(); } }
        public IDispatchCommits Dispatcher { get { return Get<IDispatchCommits>(); } }
        public IAuthUser CurrentUser { get { return Get<IUserService>().CurrentUser; } }
        public FakeHttpClient HttpClient { get { return kernel.Get<IHttpClient>() as FakeHttpClient; } }
        public CompareObjects Comparer { get { return new CompareObjects(); } }

        protected override void ExecuteCommand(IStoreEvents store, ICommand cmd)
        {

            var Cmd = (IAggregateCommand)cmd;
            //MethodInfo method = typeof(CommandHandler).GetMethod("Handle");
            //MethodInfo generic = method.MakeGenericMethod(Cmd.EntityType, Cmd.GetType());
            try
            {
                Handler.SendMessage(Cmd);
            }
            catch (TargetInvocationException e)
            {

                var inner = e.InnerException as DomainError;
                if (inner != null)
                {
                    throw inner;
                }
                throw;
            }
            //.Handle(cmd);
        }


    }
}
