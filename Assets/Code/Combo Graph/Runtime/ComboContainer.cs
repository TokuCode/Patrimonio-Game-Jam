using System.Collections.Generic;
using UnityEngine;

namespace Movement3D.ComboGraph
{
    public class ComboContainer : ScriptableObject
    {
        public List<NodeData> nodes = new();
        public List<NodeLinkData> links = new();
    }
}