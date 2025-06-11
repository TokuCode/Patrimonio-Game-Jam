using System;
using System.Collections.Generic;
using UnityEngine;

namespace Movement3D.ComboGraph
{
    [Serializable]
    public class NodeData
    {
        public SerializableGuid nodeGuid;
        public string nodeName;
        public List<string> attacks;
        public Vector2 position;
        public bool isEntryPoint;
    }
}