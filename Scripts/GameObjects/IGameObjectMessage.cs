using UnityEngine;

namespace Phaser
{
    public interface IGameObjectMessage : IMessage
    {
        GameObject GameObject { get; set; }
    }
}