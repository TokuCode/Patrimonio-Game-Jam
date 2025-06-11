using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Movement3D.ComboGraph
{
    public class ComboGraphNode : Node
    {
        public SerializableGuid guid;
        public bool entryPoint = false;
        public List<string> attacks;
        public string nodeName;
    }
}