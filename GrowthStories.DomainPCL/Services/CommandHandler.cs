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

        private readonly IGSRepository Repository;
        private readonly IAggregateFactory Factory;
        private readonly IUIPersistence UIPersistence;
        private static ILog Logger = LogFactory.BuildLogger(typeof(CommandHandler));
        //private readonly IMessageBus _bus;

        private readonly object gate = new object();

        public CommandHandler(
            IGSRepository store,
            IAggregateFactory factory,
            IUIPersistence uipersistence
            )
            : base()
        {
            Repository = store;
            Factory = factory;
            UIPersistence = uipersistence;
        }


        protected IGSAggregate Construct(IMessage c)
        {
            ICreateMessage cc = c as ICreateMessage;
            if (cc != null)
                return Construct(cc);
            return c.AggregateId == GSAppState.GSAppId ? GetApp() : Repository.GetById(c.AggregateId);

        }

        protected IGSAggregate Construct(ICreateMessage cc)
        {

            return (IGSAggregate)Factory.Build(cc.AggregateType);


        }

        protected IGSAggregate RemoteConstruct(IMessage c)
        {

            try
            {
                return Repository.GetById(c.AggregateId);
            }
            catch (DomainError e)
            {
                if (e.Name != "premature")
                    throw;

            }
            ICreateMessage cc = c as ICreateMessage;
            if (cc != null)
                return (IGSAggregate)Factory.Build(cc.AggregateType);

            throw DomainError.Named("notfound", "Cant't find stream based on remote event {0}", c.ToString());

        }

        protected IGSAggregate RemoteConstructNoThrow(IMessage c)
        {
            try
            {
                return RemoteConstruct(c);
            }
            catch
            {

            }
            return null;
        }


        public IGSAggregate Handle(IStreamSegment msgs)
        {


            var aggregate = msgs.CreateMessage != null ? Construct(msgs.CreateMessage) : Construct(msgs.First());

            foreach (var msg in msgs)
                aggregate.Handle(msg);


            GSApp A = null;
            IMessage[] appMsgs = null;
            if (!(aggregate is GSApp))
            {
                A = GetApp();
                appMsgs = msgs.Where(x => GSApp.CanHandle(x)).ToArray();
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


        public int AttachAggregates(ISyncPullResponse pullResp)
        {
            return pullResp.Streams
                .Aggregate(
                    0,
                    (acc, x) =>
                    {

                        x.Aggregate = x.CreateMessage != null ? Construct(x.CreateMessage) : RemoteConstructNoThrow(x.First());
                        x.TrimDuplicates();
                        return acc + (x.Aggregate == null ? 0 : 1);
                    }
                 );

        }

        public GSApp Handle(Pull c)
        {

            var A = GetApp();

            var remoteStreams = c.Sync.PullResp.Streams.ToArray();

            var aggregates = remoteStreams
                .Where(x => x.Aggregate != null && x.Count(y => IsRelationshipNotification(y, A.State.User)) == 0)
                .Select(x =>
                {
                    foreach (var e in x)
                        x.Aggregate.ApplyRemoteMessage(e);
                    return x.Aggregate;
                })
                .Where(x => x.GetUncommittedEvents().Count > 0)
                .ToArray();

            var UIEvents = remoteStreams
                .SelectMany(x => x)
                .Where(y => IsRelationshipNotification(y, A.State.User))
                .ToArray();

            if (UIEvents.Length > 0)
                UISave(UIEvents);

            if (aggregates.Length > 0)
                Save(aggregates);

            c.GlobalCommitSequence = Repository.GetGlobalCommitSequence();

            // let App handle EVERY incoming message
            remoteStreams
                .SelectMany(x => x)
                .Where(x => GSApp.CanHandle(x, true))
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

        private void UISave(IMessage[] UIEvents)
        {
            foreach (var e in UIEvents)
            {
                bool? status = null;
                if (e is CollaborationRequested)
                    status = true;
                if (e is CollaborationDenied)
                    status = false;
                if (status.HasValue)
                    UIPersistence.SaveCollaborator(e.AggregateId, status.Value);
            }
        }

        private bool IsRelationshipNotification(IMessage e, IAuthUser u)
        {
            var re = e as RelationshipEvent;
            if (re != null && re.Target == u.Id)
                return true;


            return false;
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
