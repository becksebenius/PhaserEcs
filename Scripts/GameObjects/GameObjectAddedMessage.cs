using UnityEngine;

namespace Phaser
{
    public struct GameObjectAddedMessage : IGameObjectMessage
    {
        public GameObject GameObject { get; set; }
    }
}