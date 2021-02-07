namespace Phaser
{
    public delegate void MessageListenerDelegate<TMessage>(in TMessage message) where TMessage : struct, IMessage;
}