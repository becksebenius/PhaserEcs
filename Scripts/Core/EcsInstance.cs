using System;
using System.Collections.Generic;
using System.Linq;

namespace Phaser
{
    public class EcsInstance : IMessageHandler
    {
        public event Action<string> OnError;
        public IReadOnlyList<object> Systems => systems;

        private readonly IEcsSystem[] systems;
        private readonly MessageBuffer deferredMessageBuffer = MessageBuffer.New();

        public EcsInstance(IReadOnlyList<IEcsSystem> systems)
        {
            this.systems = systems.ToArray();
            for(int i = 0; i < this.systems.Length; ++i)
            {
                var system = this.systems[i];
                system.Initialize(this);
            }
        }

        /// <summary>
        /// Sends a message to be handled just-in-time when processing systems
        /// </summary>
        public void SendMessage<TMessage>(in TMessage message) where TMessage : struct, IMessage
        {
            for (int i = 0; i < systems.Length; ++i)
            {
                systems[i].QueueMessage(in message);
            }
        }

        /// <summary>
        /// Sends a message to be handled after the current message pass is completed
        /// </summary>
        public void SendMessageDeferred<TMessage>(in TMessage message) where TMessage : struct, IMessage
        {
            deferredMessageBuffer.QueueMessage(in message);
        }

        /// <summary>
        /// Runs an update pass on the ecs instance, which includes queuing the input message
        ///     as a deferred message and then processing all deferred messages
        /// Any pending deferred messages will be processed before the update,
        ///     and any deferred messages queued during the update pass will be run
        ///     until no more messages have been added
        /// </summary>
        public void Update<TMessage>(in TMessage message) where TMessage : struct, IMessage
        {
            SendMessageDeferred(message);

            int recursionGuard = 100;
            while (deferredMessageBuffer.HandleNext(this))
            {
                if(recursionGuard-- <= 0)
                {
                    OnError?.Invoke("100+ deferred messages processed. Probably infinite recursion!");
                    break;
                }
            }

            recursionGuard = 100;
            while (ProcessOutstandingQueuedMessages())
            {
                if(recursionGuard-- <= 0)
                {
                    OnError?.Invoke("100+ queued messages processed. Probably infinite recursion!");
                    break;
                }
            }
        }

        /// <summary>
        /// Returns the system instance in this EcsInstance
        /// </summary>
        public TSystem GetSystem<TSystem>() where TSystem : SystemBase
        {
            for (int i = 0; i < systems.Length; ++i)
            {
                if(systems[i] is TSystem system)
                {
                    return system;
                }
            }
            return null;
        }

        bool ProcessOutstandingQueuedMessages()
        {
            bool anyProcessed = false;
            for(int i = 0; i < systems.Length; ++i)
            {
                var system = systems[i];
                anyProcessed |= system.HandleQueuedMessages();
            }
            return anyProcessed;
        }

        /// <summary>
        /// Synchronously handles the given message on all systems in the Ecs instance
        /// </summary>
        void IMessageHandler.HandleMessage<TMessage>(in TMessage message)
        {
            for (int i = 0; i < systems.Length; ++i)
            {
                var system = systems[i];
                system.HandleQueuedMessages();
                system.HandleMessage(in message);
            }
        }
    }
}