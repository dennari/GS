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
        private readonly ISynchronizerService SyncService;
        private readonly ITransportEvents Transporter;
        private readonly IRequestFactory RequestFactory;

        public IGSApp App { get; set; }
        //private readonly IUIPersistence UIPersistence;
        private readonly object gate = new object();

        public CommandHandler(
            IGSRepository store,
            IAggregateFactory factory,
            ITransportEvents transporter,
            IRequestFactory requestFactory,
            ISynchronizerService syncService
            )
            : base()
        {
            Repository = store;
            SyncService = syncService;
            //_persistence = persistence;
            Factory = factory;
            Transporter = transporter;
            RequestFactory = requestFactory;


        }



        protected IGSAggregate Construct(IMessage c)
        {
            ICreateMessage cc = c as ICreateMessage;
            return cc == null ? Repository.GetById(c.AggregateId) : (IGSAggregate)Factory.Build(cc.AggregateType);

        }


        public IGSAggregate Handle(IAggregateMessages msgs)
        {


            var aggregate = Construct(msgs.Messages[0]);

            foreach (var msg in msgs.Messages)
                aggregate.Handle(msg);

            Repository.Save(aggregate);

            return aggregate;

        }


        protected void Save(IGSAggregate[] gs)
        {
            lock (this.gate)
            {
                Repository.Save(gs);
            }
        }

        public Task<List<IGSAggregate>> Handle(Synchronize c)
        {
            if (this.App == null)
                throw new InvalidOperationException("No knowledge of the GSApp aggregate, which is needed for sync.");


            var s = SyncService.Synchronize(RequestFactory.CreatePullRequest(App.State.SyncStreams.ToArray()), RequestFactory.CreatePushRequest());

            if (s.PullReq.IsEmpty && s.PushReq.IsEmpty)
                return null;

            return Task.Run(async () =>
            {
                var pullResp = await s.Pull();
                if (pullResp != null && pullResp.StatusCode == GSStatusCode.OK)
                {

                }


                await s.Push();

                return new List<IGSAggregate>();
            });

            //return this.App;
        }
    }
}
