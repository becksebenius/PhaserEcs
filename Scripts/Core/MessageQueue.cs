using System.Collections.Generic;

namespace Phaser
{
    public class MessageQueue<TMessage> : IMessageQueue
        where TMessage : struct, IMessage
    {
        private List<TMessage> messages = new List<TMessage>();

        public void Enqueue(in TMessage message)
        {
            messages.Add(message);
        }

        public void HandleAndRemoveNext(IMessageHandler handler)
        {
            var message = messages[0];
            messages.RemoveAt(0);
            handler.HandleMessage(in message);
        }
    }
}