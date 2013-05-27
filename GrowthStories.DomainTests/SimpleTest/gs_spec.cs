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

namespace Growthstories.DomainTests
{
    public class gs_spec : spec_syntax
    {
        IKernel kernel;
        Boolean binded = false;

        protected Guid id = Guid.NewGuid();

        protected override Guid Id
        {
            get
            {
                return id;
            }
        }

        protected virtual ICommandHandler<ICommand> GetCommandHandler(IStoreEvents store)
        {

            return kernel.Get<ICommandHandler<ICommand>>(new Parameter("store", store, true));

        }

        protected override IStoreEvents GetEventStore()
        {
            return kernel.Get<IStoreEvents>();
        }


        protected override void SetupServices()
        {
            if (binded)
            {
                return;
            }

            kernel = new StandardKernel();
            kernel.WireUp();
            binded = true;
        }

        protected override void ExecuteCommand(IStoreEvents store, ICommand cmd)
        {

            this.GetCommandHandler(store).Handle(cmd);
        }

        protected JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto
        };

        protected virtual void Dispatch(Commit commit)
        {
            try
            {
                //Console.WriteLine(JsonConvert.SerializeObject(commit, SerializerSettings));
                //foreach (var @event in commit.Events)
                //    Console.WriteLine(string.Format("dispatch: {0}, {1}", @event.Body.GetType(), ((EventBase)@event.Body).EntityId));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
