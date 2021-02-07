namespace Phaser
{
    public interface IMessageQueue
    {
        void HandleAndRemoveNext(IMessageHandler messageHandler);
    }
}