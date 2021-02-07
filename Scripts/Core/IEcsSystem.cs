namespace Phaser
{
    public interface IEcsSystem : IMessageHandler
    {
        void Initialize(EcsInstance instance);
        void QueueMessage<TMessage>(in TMessage message) where TMessage : struct, IMessage;
        bool HandleQueuedMessages();
    }
}