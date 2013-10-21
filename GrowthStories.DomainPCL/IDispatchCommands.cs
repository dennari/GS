using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public interface IDispatchCommands
    {


        IGSAggregate Handle(IMessage c);
        IGSAggregate Handle(IAggregateMessages msgs);
        GSApp Handle(Pull c);
        GSApp Handle(Push c);


    }


    public interface IUIPersistence
    {

        void Purge();




        IEnumerable<PlantActionState> GetActions(Guid? PlantActionId = null, Guid? PlantId = null, Guid? UserId = null);
        IEnumerable<PlantState> GetPlants(Guid? PlantId = null, Guid? GardenId = null, Guid? UserId = null);
        IEnumerable<UserState> GetUsers(Guid? UserId = null);

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

    public static class MBExtensions
    {


        public static void SendCommand<T>(this IMessageBus bus, T message)
            where T : IAggregateCommand
        {
            bus.SendMessage((IAggregateCommand)message);
        }

        public static void SendCommands(this IMessageBus bus, IAggregateMessages msgs)
        {
            bus.SendMessage(msgs);
        }
    }


}
