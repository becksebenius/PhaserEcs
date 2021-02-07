using UnityEngine;

namespace Phaser
{
    public class LazyDestroyGameObjectSystem : SystemBase
    {
        public LazyDestroyGameObjectSystem()
        {
            AddMessageListener<LazyDestroyGameObjectMessage>(OnLazyDestroyGameObject);
        }

        private void OnLazyDestroyGameObject(in LazyDestroyGameObjectMessage message)
        {
            if (message.GameObject)
            {
                var destroyedStateComponent = message.GameObject.GetComponent<DestroyedStateComponent>();
                if (destroyedStateComponent)
                {
                    if (destroyedStateComponent.isDestroyed)
                    {
                        return;
                    }

                    destroyedStateComponent.isDestroyed = true;
                }

                Object.Destroy(message.GameObject);
            }
        }
    }
}