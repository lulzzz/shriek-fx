namespace Shriek.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        void Execute(ICommandContext context, TCommand command);
    }
}