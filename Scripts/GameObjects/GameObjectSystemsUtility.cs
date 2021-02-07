using System.Collections.Generic;

namespace Phaser
{
    public static class GameObjectSystemsUtility
    {
        public static IEnumerable<IEcsSystem> GetInitializationSystems() => new IEcsSystem[]
        {
            new GameObjectRemovalSystem()
        };
    }
}