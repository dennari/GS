using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using EventStore.Logging;
using EventStore.Persistence;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
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

        readonly IGSRepository Repository;
        readonly IAggregateFactory Factory;
        //private readonly IPersistSyncStreams _persistence;
        private static ILog Logger = LogFactory.BuildLogger(typeof(CommandHandler));
        //private readonly IMessageBus _bus;

        private readonly object gate = new object();

        public CommandHandler(
            IGSRepository store,
            IAggregateFactory factory
            )
            : base()
        {
            Repository = store;
            Factory = factory;
        }


        protected IGSAggregate Construct(IMessage c)
        {
            ICreateMessage cc = c as ICreateMessage;
            if (cc != null)
                return (IGSAggregate)Factory.Build(cc.AggregateType);
            return c.AggregateId == GSAppState.GSAppId ? GetApp() : Repository.GetById(c.AggregateId);

        }


        public IGSAggregate Handle(IAggregateMessages msgs)
        {


            var aggregate = Construct(msgs.Messages[0]);

            foreach (var msg in msgs.Messages)
                aggregate.Handle(msg);


            GSApp A = null;
            IMessage[] appMsgs = null;
            if (!(aggregate is GSApp))
            {
                A = GetApp();
                appMsgs = msgs.Messages.Where(x => GSApp.CanHandle(x)).ToArray();
                foreach (var msg in appMsgs)
                    A.Handle(msg);
            }

            if (A != null && appMsgs != null && appMsgs.Length > 0)
                Save(new[] { aggregate, A });
            else
                Repository.Save(aggregate);


            return aggregate;

        }

        protected GSApp _App;
        public GSApp GetApp()
        {
            return _App ?? (_App = (GSApp)Repository.GetById(GSAppState.GSAppId));
        }

        //public 

        public IGSAggregate Handle(IMessage msg)
        {


            var aggregate = Construct(msg);

            aggregate.Handle(msg);

            GSApp A = null;
            if (!(aggregate is GSApp))
            {
                A = GetApp();
                if (GSApp.CanHandle(msg))
                    A.Handle(msg);
            }

            if (A != null && GSApp.CanHandle(msg))
                Save(new[] { aggregate, A });
            else
                Repository.Save(aggregate);

            return aggregate;

        }


        public GSApp Handle(Pull c)
        {

            var remoteStreams = c.Sync.PullResp.Streams
                .Select(x => { x.Aggregate = Construct(x.Messages[0]); return x; })
                .ToArray();

            var aggregates = remoteStreams
                .Select(x => c.Sync.HandleRemoteMessages(x))
                .Where(x => x.GetUncommittedEvents().Count > 0);



            Save(aggregates.ToArray());

            c.GlobalCommitSequence = Repository.GetGlobalCommitSequence();

            // let App handle EVERY incoming message            
            var A = GetApp();
            remoteStreams
                .SelectMany(x => x.Messages)
                .Where(x => GSApp.CanHandle(x))
                .Aggregate(A, (X, m) => { X.Handle(m); return X; });

            A.Handle(c);

            Repository.Save(A);

            return A;

        }

        public GSApp Handle(Push c)
        {
            c.GlobalCommitSequence = Repository.GetGlobalCommitSequence();
            return (GSApp)this.Handle((IMessage)c);
        }



        protected void Save(IGSAggregate[] gs)
        {
            lock (this.gate)
            {
                Repository.Save(gs);
            }
        }



    }
}
