using System.Collections.Generic;
using UnityEngine;

namespace Phaser
{
    public struct EvaluateEcsHierarchies : IMessage {}
    public struct EvaluateEcsHierarchy : IGameObjectMessage
    {
        public GameObject GameObject { get; set; }
    }

    public class EcsHierarchySystem : GameObjectSystem<EcsHierarchySystem.ComponentSet>
    {
        public struct ComponentSet
        {
            public Transform Transform;
            public EcsHierarchyRoot EcsHierarchyRoot;
        }

        private static HashSet<GameObject> reuseableHashSet = new HashSet<GameObject>();

        public EcsHierarchySystem()
        {
            AddSingleObjectMessageListener<GameObjectRemovedMessage>(OnGameObjectRemoved);
            AddPerObjectMessageListener<EvaluateEcsHierarchies>(OnEvaluateAllEcsHierarchies);
            AddSingleObjectMessageListener<EvaluateEcsHierarchy>(OnEvaluateSingleEcsHierarchy);
        }

        private void OnGameObjectRemoved(GameObject gameObject, in ComponentSet components, in GameObjectRemovedMessage message)
        {
            if(components.EcsHierarchyRoot.trackedObjectsHashed.Remove(gameObject))
            {
                components.EcsHierarchyRoot.trackedObjects.Remove(gameObject);
            }
        }

        private void OnEvaluateAllEcsHierarchies(GameObject gameObject, in ComponentSet components, in EvaluateEcsHierarchies message)
            => EvaluateEcsHierarchy(in components);

        private void OnEvaluateSingleEcsHierarchy(GameObject gameObject, in ComponentSet components, in EvaluateEcsHierarchy message)
            => EvaluateEcsHierarchy(in components);

        private void EvaluateEcsHierarchy(in ComponentSet components)
        {
            int children = components.Transform.childCount;
            int existingGameObjectCount = components.EcsHierarchyRoot.trackedObjects.Count;

            var actualChildren = reuseableHashSet;
            
            for (int i = 0; i < children; ++i)
            {
                var child = components.Transform.GetChild(i);
                var childGameObject = child.gameObject;
                if (!childGameObject.activeSelf)
                {
                    continue;
                }

                if (components.EcsHierarchyRoot.trackedObjectsHashed.Add(childGameObject))
                {
                    EcsInstance.AddGameObject(childGameObject);
                    components.EcsHierarchyRoot.trackedObjects.Add(childGameObject);
                }

                actualChildren.Add(childGameObject);
            }

            for (int i = 0; i < existingGameObjectCount; ++i)
            {
                var trackedObject = components.EcsHierarchyRoot.trackedObjects[i];
                if (actualChildren.Add(trackedObject))
                {
                    EcsInstance.RemoveGameObject(trackedObject);
                    components.EcsHierarchyRoot.trackedObjectsHashed.Remove(trackedObject);
                    components.EcsHierarchyRoot.trackedObjects.RemoveAt(i);
                    i--;
                    existingGameObjectCount--;
                }
            }

            actualChildren.Clear();
        }
    }
}