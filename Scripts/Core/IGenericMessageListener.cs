namespace Phaser
{
    public interface IGenericMessageListener
    {
        void OnMessage<TMessage>(in TMessage message) where TMessage : struct, IMessage;
    }
}