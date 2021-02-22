using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phaser
{
    public interface IGameObjectSystem
    {
        int NumGameObjects { get; }
        GameObject GetGameObject(int id);
    }

    public class OptionalAttribute : Attribute { }
    public class FromParentAttribute : Attribute { }

    public abstract class GameObjectSystem<TComponentSet> : SystemBase, IGameObjectSystem
        where TComponentSet : struct
    {       
        public struct SystemObject
        {
            public readonly GameObject GameObject;
            public readonly TComponentSet Components;

            public SystemObject(GameObject gameObject, TComponentSet components)
            {
                GameObject = gameObject;
                Components = components;
            }
        }

        protected delegate void GameObjectMessageListenerDelegate<TMessage>(
            GameObject gameObject,
            in TComponentSet components, 
            in TMessage message)
            where TMessage : struct, IMessage;

        private class PerGameObjectMessageListenerWrapper<TMessage>
            where TMessage : struct, IMessage
        {
            private readonly GameObjectSystem<TComponentSet> parent;
            private readonly GameObjectMessageListenerDelegate<TMessage> messageListener;

            public PerGameObjectMessageListenerWrapper(GameObjectSystem<TComponentSet> parent, GameObjectMessageListenerDelegate<TMessage> messageListener)
            {
                this.parent = parent;
                this.messageListener = messageListener;
            }

            public void Invoke(in TMessage message)
            {
                for(int i = 0; i < parent.objects.Count; ++i)
                {
                    var obj = parent.objects[i];
                    messageListener(obj.GameObject, in obj.Components, in message);
                }
            }
        }

        private class SingleGameObjectMessageListenerWrapper<TMessage>
            where TMessage : struct, IGameObjectMessage
        {
            private readonly GameObjectSystem<TComponentSet> parent;
            private readonly GameObjectMessageListenerDelegate<TMessage> messageListener;

            public SingleGameObjectMessageListenerWrapper(GameObjectSystem<TComponentSet> parent, GameObjectMessageListenerDelegate<TMessage> messageListener)
            {
                this.parent = parent;
                this.messageListener = messageListener;
            }

            public void Invoke(in TMessage message)
            {
                for(int i = 0; i < parent.objects.Count; ++i)
                {
                    var obj = parent.objects[i];
                    if(obj.GameObject == message.GameObject)
                    {
                        messageListener(obj.GameObject, in obj.Components, in message);
                    }
                }
            }
        }

        public IReadOnlyList<SystemObject> Objects => objects;
        private readonly List<SystemObject> objects = new List<SystemObject>();

        public GameObjectSystem()
        {
            AddMessageListener<GameObjectAddedMessage>(OnGameObjectAdded);
            AddMessageListener<GameObjectRemovedMessage>(OnGameObjectRemoved);
        }

        private void OnGameObjectAdded(in GameObjectAddedMessage message)
        {
            if(ComponentSetBuilder<TComponentSet>.TryCreateComponentSet(message.GameObject, out TComponentSet componentSet))
            {
                objects.Add(new SystemObject(message.GameObject, componentSet));
                OnObjectAdded(message.GameObject, in componentSet);
            }
        }

        private void OnGameObjectRemoved(in GameObjectRemovedMessage message)
        {
            for(int i = 0; i < objects.Count; ++i)
            {
                var obj = objects[i];
                if (obj.GameObject == message.GameObject)
                {
                    objects.RemoveAt(i--);
                    OnObjectRemoved(obj.GameObject, in obj.Components);
                }
            }
        }

        protected virtual void OnObjectRemoved(GameObject gameObject, in TComponentSet components)
        {
        }

        protected virtual void OnObjectAdded(GameObject gameObject, in TComponentSet components)
        {
        }
        
        protected void AddPerObjectMessageListener<TMessage>(GameObjectMessageListenerDelegate<TMessage> messageListener) 
            where TMessage : struct, IMessage
        {
            AddMessageListener<TMessage>(new PerGameObjectMessageListenerWrapper<TMessage>(this, messageListener).Invoke);
        }

        protected void AddSingleObjectMessageListener<TMessage>(GameObjectMessageListenerDelegate<TMessage> messageListener)
            where TMessage : struct, IGameObjectMessage
        {
            AddMessageListener<TMessage>(new SingleGameObjectMessageListenerWrapper<TMessage>(this, messageListener).Invoke);
        }

        int IGameObjectSystem.NumGameObjects => objects.Count;
        GameObject IGameObjectSystem.GetGameObject(int id) => objects[id].GameObject;
    }
}