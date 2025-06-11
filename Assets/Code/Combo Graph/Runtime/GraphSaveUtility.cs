using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Movement3D.ComboGraph
{
    public class GraphSaveUtility
    {
        private ComboGraphView _targetGraphView;
        private ComboContainer _cacheContainer;

        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<ComboGraphNode> Nodes => _targetGraphView.nodes.ToList().Cast<ComboGraphNode>().ToList();
        
        public static GraphSaveUtility GetInstance(ComboGraphView targetGraphView)
        {
            return new GraphSaveUtility
            {
                _targetGraphView = targetGraphView
            };
        }

        private bool SaveNodes(ComboContainer container)
        {
            if (!Edges.Any()) return false;

            var connectedPorts = Edges.Where(edge => edge.input.node != null).ToArray();
            for (int i = 0; i < connectedPorts.Length; i++)
            {
                var port = connectedPorts[i];
                
                var outputNode = port.output.node as ComboGraphNode;
                var inputNode = port.input.node as ComboGraphNode;
                
                container.links.Add(new NodeLinkData
                {
                    sourceNodeGuid = outputNode.guid,
                    targetNodeGuid = inputNode.guid,
                    tag = port.output.portName
                });
            }
            
            foreach (var node in Nodes)
            {
                container.nodes.Add(new NodeData
                {
                    nodeGuid = node.guid,
                    nodeName = node.nodeName,
                    position = node.GetPosition().position,
                    attacks = node.attacks,
                    isEntryPoint = node.entryPoint
                });
            }

            return true;
        }

        public void SaveGraph(string fileName)
        {
            var dialogueContainer = ScriptableObject.CreateInstance<ComboContainer>();
            if (!SaveNodes(dialogueContainer)) return;
     
            if(!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            
            if(!AssetDatabase.IsValidFolder("Assets/Resources/ComboGraph"))
                AssetDatabase.CreateFolder("Assets/Resources", "ComboGraph");
            
            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/ComboGraph/{fileName}.asset");
            AssetDatabase.SaveAssets();
        }
        
        public void LoadGraph(string fileName)
        {
            _cacheContainer = Resources.Load<ComboContainer>($"ComboGraph/{fileName}");
            if (_cacheContainer == null)
            {
                EditorUtility.DisplayDialog("File not found", "Target dialogue graph file does not exist!", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
        }

        private void ClearGraph()
        {
            Nodes.Find(node => node.entryPoint).guid = _cacheContainer.links[0].sourceNodeGuid;

            foreach (var node in Nodes)
            {
                if (node.entryPoint) continue;
                
                Edges.Where(edge => edge.input.node == node).ToList()
                    .ForEach(edge => _targetGraphView.RemoveElement(edge));
                
                _targetGraphView.RemoveElement(node);
            }
            _targetGraphView.ResetEntry();
        }

        private void CreateNodes()
        {
            foreach (var node in _cacheContainer.nodes)
            {
                ComboGraphNode tempNode;
                if (!node.isEntryPoint)
                {
                    tempNode = _targetGraphView.CreateComboNode(node.nodeName, Vector2.zero, node.attacks);
                    tempNode.guid = node.nodeGuid;
                    _targetGraphView.AddElement(tempNode);
                }
                else tempNode = _targetGraphView.entryPointNode;
                
                var nodePorts = _cacheContainer.links.Where(link => link.sourceNodeGuid == node.nodeGuid).ToList();
                nodePorts.ForEach(link => _targetGraphView.AddChoicePort(tempNode, link.tag));
            }
        }

        private void ConnectNodes()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                var connections = _cacheContainer.links.Where(link => link.sourceNodeGuid == Nodes[i].guid).ToList();
                for (var j = 0; j < connections.Count; j++)
                {
                    var link = connections[j];
                    var targetNodeGuid = link.targetNodeGuid;
                    var targetNode = Nodes.First(connection => connection.guid == targetNodeGuid);
                    
                    LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);
                    targetNode.SetPosition(new Rect(
                        _cacheContainer.nodes.First(node => node.nodeGuid == targetNodeGuid).position,
                        _targetGraphView.defaultNodeSize));
                }
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };
            
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            _targetGraphView.AddElement(tempEdge);
        }
    }
}