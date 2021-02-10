namespace Phaser
{
    public abstract class SystemBase : IEcsSystem
    {
        protected EcsInstance EcsInstance { get; private set; }

        private readonly MessageListenerCollection messageListeners = MessageListenerCollection.New();
        private MessageBuffer? queuedMessageBuffer;
        private IGenericMessageListener thisAsGenericMessageListener;

        protected SystemBase()
        {
            thisAsGenericMessageListener = this as IGenericMessageListener;
        }

        protected void AddMessageListener<TMessage>(MessageListenerDelegate<TMessage> listener) where TMessage : struct, IMessage
        {
            messageListeners.AddMessageListener(listener);
        }

        void IMessageHandler.HandleMessage<TMessage>(in TMessage message) => HandleMessage(in message);
        private void HandleMessage<TMessage>(in TMessage message) where TMessage : struct, IMessage
        {
            messageListeners.Invoke(message);
            thisAsGenericMessageListener?.OnMessage(in message);
        }

        void IEcsSystem.QueueMessage<TMessage>(in TMessage message)
        {
            if(thisAsGenericMessageListener == null && !messageListeners.HasListenerForType<TMessage>())
            {
                return;
            }
            if(!queuedMessageBuffer.HasValue)
            {
                queuedMessageBuffer = MessageBuffer.New();
            }
            queuedMessageBuffer.Value.QueueMessage(in message);
        }

        bool IEcsSystem.HandleQueuedMessages()
        {
            bool anyProcessed = false;

            if (queuedMessageBuffer.HasValue)
            {
                while (queuedMessageBuffer.Value.HandleNext(this))
                {
                    anyProcessed = true;
                };
            }

            return anyProcessed;
        }

        void IEcsSystem.Initialize(EcsInstance instance)
        {
            EcsInstance = instance;
        }
    }
}