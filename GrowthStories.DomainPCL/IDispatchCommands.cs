using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public interface IDispatchCommands
    {


        Task<IGSAggregate> Handle(IMessage c);
        Task<IGSAggregate> Handle(IStreamSegment msgs);
        int AttachAggregates(ISyncPullResponse pullResp);
        void ResetApp();
        Task<GSApp> Handle(Pull c);
        Task<GSApp> Handle(Push c);


    }


    public interface IUIPersistence : IHasLogger
    {

        void Purge();

        void Initialize();

        void ReInitialize();
        //PlantActionState GetLatestWatering(Guid? PlantId);

        IEnumerable<PlantActionState> GetActions(
            Guid? PlantActionId = null,
            Guid? PlantId = null,
            Guid? UserId = null,
            PlantActionType? type = null,
            int? limit = null,
            bool? isOrderAsc = null
            );

        IEnumerable<Tuple<PlantState, ScheduleState, ScheduleState>> GetPlants(Guid? PlantId = null, Guid? GardenId = null, Guid? UserId = null);
        IEnumerable<UserState> GetUsers(Guid[] UserIds = null);
        //IEnumerable<PlantActionState> GetPhotoActions(Guid? PlantId = null);

        //IEnumerable<ScheduleState> GetSchedules(Guid PlantId);


        //IEnumerable<ActionBase> UserActions(Guid UserId);
        //IEnumerable<PlantCreated> UserPlants(Guid UserId);

        void Save(IGSAggregate aggregate);
        void SaveCollaborator(Guid collaboratorId, bool status);

    }

    //public interface IRegisterHandlers
    //{
    //    IDictionary<Tuple<Type, Type>, Action<IGSAggregate, IAggregateCommand>> RegisterHandlers();
    //    IDictionary<Tuple<Type, Type>, Func<IGSAggregate, IAggregateCommand, Task<object>>> RegisterAsyncHandlers();

    //}

    //public interface IRegisterEventHandlers
    //{
    //    void Register<TEvent>(IEventHandler<TEvent> handler)
    //        where TEvent : IEvent;

    //    void RegisterAsync<TEvent>(IAsyncEventHandler<TEvent> handler)
    //        where TEvent : IEvent;

    //}

    //public interface IRegisterCommandHandlers
    //{
    //    void Register<TCommand>(ICommandHandler<TCommand> handler)
    //        where TCommand : ICommand;

    //    void RegisterAsync<TCommand>(IAsyncCommandHandler<TCommand> handler)
    //        where TCommand : ICommand;

    //}

    public static class Extensions
    {


        //public static void SendCommand<T>(this IMessageBus bus, T message)
        //    where T : IAggregateCommand
        //{
        //    bus.SendMessage((IAggregateCommand)message);
        //}

        //public static void SendCommands(this IMessageBus bus, IStreamSegment msgs)
        //{
        //    bus.SendMessage(msgs);
        //}

        //public static void SendCommands(this IMessageBus bus, params IAggregateCommand[] msgs)
        //{
        //    bus.SendCommands(msgs.GroupBy(x => x.AggregateId).Select(x => new StreamSegment(x)).Single());
        //}

        public static void MergeByCreated(this IAggregateState st, IMessage incoming, IMessage outgoing, out IMessage incomingNew, out IMessage outgoingNew)
        {
            if (incoming.Created >= outgoing.Created)
            {
                outgoingNew = new NullEvent(outgoing);
                incomingNew = incoming;
            }
            else
            {
                outgoingNew = outgoing;
                incomingNew = new NullEvent(incoming);
            }
        }

    }


}
