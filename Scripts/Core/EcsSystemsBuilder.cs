using System.Collections.Generic;

namespace Phaser
{
    public class EcsSystemsBuilder
    {
        private readonly List<IEcsSystem> systemsList = new List<IEcsSystem>();

        public EcsSystemsBuilder AddSystem(IEcsSystem system)
        {
            systemsList.Add(system);
            return this;
        }

        public EcsSystemsBuilder AddSystems(IEnumerable<IEcsSystem> systems)
        {
            systemsList.AddRange(systems);
            return this;
        }

        public IEcsSystem[] Build() => systemsList.ToArray();
    }
}