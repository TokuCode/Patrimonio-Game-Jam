using System;

namespace Movement3D.ComboGraph
{
    [Serializable]
    public class NodeLinkData
    {
        public SerializableGuid sourceNodeGuid;
        public SerializableGuid targetNodeGuid;
        public string tag;
    }
}