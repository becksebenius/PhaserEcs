namespace Phaser
{
    public interface IMessageHandler
    {
        void HandleMessage<TMessage>(in TMessage message) where TMessage : struct, IMessage;
    }
}