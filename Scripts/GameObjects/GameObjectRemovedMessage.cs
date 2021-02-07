using UnityEngine;

namespace Phaser
{
    public struct GameObjectRemovedMessage : IGameObjectMessage
    {
        public GameObject GameObject { get; set; }
    }
}