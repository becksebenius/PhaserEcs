using System.Collections.Generic;
using UnityEngine;

namespace Phaser
{
    public class GameObjectRemovalSystem : SystemBase, IGenericMessageListener
    {
        private List<GameObject> gameObjects = new List<GameObject>();

        public GameObjectRemovalSystem()
        {
            AddMessageListener<GameObjectAddedMessage>(OnGameObjectAdded);
            AddMessageListener<GameObjectRemovedMessage>(OnGameObjectRemoved);
        }

        void IGenericMessageListener.OnMessage<TMessage>(in TMessage message)
        {
            if(typeof(TMessage) == typeof(GameObjectAddedMessage)
            || typeof(TMessage) == typeof(GameObjectRemovedMessage))
            {
                return;
            }

            for(int i = 0; i < gameObjects.Count; ++i)
            {
                var gameObject = gameObjects[i];
                if(!gameObject)
                {
                    gameObjects.RemoveAt(i--);
                    EcsInstance.SendMessage(new GameObjectRemovedMessage()
                    {
                        GameObject = gameObject
                    });
                }
            }
        }

        void OnGameObjectAdded(in GameObjectAddedMessage message)
        {
            gameObjects.Add(message.GameObject);
        }

        void OnGameObjectRemoved(in GameObjectRemovedMessage message)
        {
            gameObjects.Remove(message.GameObject);
        }
    }
}