using UnityEngine;

namespace Phaser
{
    public static class EcsInstanceGameObjectExtensions
    {
        public static void AddGameObject(this EcsInstance ecsInstance, GameObject gameObject)
            => ecsInstance.SendMessage(new GameObjectAddedMessage() { GameObject = gameObject });

        public static void RemoveGameObject(this EcsInstance ecsInstance, GameObject gameObject)
            => ecsInstance.SendMessage(new GameObjectRemovedMessage() { GameObject = gameObject });
    }
}