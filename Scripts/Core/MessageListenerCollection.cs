using System.Collections.Generic;

namespace Phaser
{
    public struct MessageListenerCollection
    {
        public static MessageListenerCollection New() => new MessageListenerCollection()
        {
            messageListeners = new List<object>()
        };

        private List<object> messageListeners;

        public void AddMessageListener<TMessage>(MessageListenerDelegate<TMessage> messageListener) where TMessage : struct, IMessage
        {
            messageListeners.Add(messageListener);
        }

        public void Invoke<TMessage>(in TMessage message) where TMessage : struct, IMessage
        {
            for (int i = 0; i < messageListeners.Count; ++i)
            {
                if (messageListeners[i] is MessageListenerDelegate<TMessage> listener)
                {
                    listener.Invoke(message);
                }
            }
        }

        public bool HasListenerForType<TMessage>() where TMessage : struct, IMessage
        {
            for(int i = 0; i < messageListeners.Count; ++i)
            {
                if(messageListeners[i] is MessageListenerDelegate<TMessage>)
                {
                    return true;
                }
            }
            return false;
        }
    }
}