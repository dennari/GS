using CommonDomain;
namespace Growthstories.Core
{

    public interface ICommandHandler
    {
        void Handle(ICommand command);
    }

    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
    }

}
