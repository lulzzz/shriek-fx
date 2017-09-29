namespace Shriek.Messages
{
    public interface IMessage { }

    public class Message : IMessage
    {
        public Message()
        {
            this.MessageType = this.GetType().AssemblyQualifiedName;
        }

        public string MessageType { get; set; }
    }
}