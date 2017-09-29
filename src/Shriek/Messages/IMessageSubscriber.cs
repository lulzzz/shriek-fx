namespace Shriek.Messages
{
    public interface IMessageSubscriber<TMessage> where TMessage : IMessage
    {
        void Execute(TMessage e);
    }
}