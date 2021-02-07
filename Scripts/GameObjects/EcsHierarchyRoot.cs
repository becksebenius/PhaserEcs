using System.Collections.Generic;
using UnityEngine;

namespace Phaser
{
    public class EcsHierarchyRoot : MonoBehaviour
    {
        public HashSet<GameObject> trackedObjectsHashed = new HashSet<GameObject>();
        public List<GameObject> trackedObjects = new List<GameObject>();
    }
}