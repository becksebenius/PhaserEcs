using UnityEngine;

namespace Phaser
{
    public struct LazyDestroyGameObjectMessage : IGameObjectMessage
    {
        public GameObject GameObject { get; set; }
    }
}