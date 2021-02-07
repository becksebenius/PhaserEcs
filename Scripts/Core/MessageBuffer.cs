using System;
using System.Collections.Generic;

namespace Phaser
{
    public struct MessageBuffer
    {
        public static MessageBuffer New() => new MessageBuffer()
        {
            messageQueuesByType = new Dictionary<Type, IMessageQueue>(),
            upcomingMessages = new List<IMessageQueue>()
        };

        private Dictionary<Type, IMessageQueue> messageQueuesByType;
        private List<IMessageQueue> upcomingMessages;

        public void QueueMessage<TMessage>(in TMessage message) where TMessage : struct, IMessage
        {
            MessageQueue<TMessage> messageQueue;
            if (messageQueuesByType.TryGetValue(typeof(TMessage), out IMessageQueue messageQueueUntyped))
            {
                messageQueue = (MessageQueue<TMessage>)messageQueueUntyped;
            }
            else
            {
                messageQueue = new MessageQueue<TMessage>();
                messageQueuesByType.Add(typeof(TMessage), messageQueue);
            }

            messageQueue.Enqueue(in message);
            upcomingMessages.Add(messageQueue);
        }

        public bool HandleNext(IMessageHandler messageHandler)
        {
            if (upcomingMessages.Count == 0)
            {
                return false;
            }

            var next = upcomingMessages[0];
            upcomingMessages.RemoveAt(0);
            next.HandleAndRemoveNext(messageHandler);
            return true;
        }
    }
}