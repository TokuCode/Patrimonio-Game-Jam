using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Movement3D.ComboGraph
{
    public class ComboGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new (150, 200);
        public ComboGraphNode entryPointNode {get; private set;}
        
        public ComboGraphView(EditorWindow editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Stylesheets/ComboGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);
            
            GenerateEntryPointNode();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if(port != startPort && port.node != startPort.node)
                    compatiblePorts.Add(port);
            });
            
            return compatiblePorts;
        }

        public void GenerateEntryPointNode()
        {
            var node = new ComboGraphNode
            {
                title = "IDLE",
                guid = SerializableGuid.NewGuid(),
                nodeName = "ENTRYPOINT",
                entryPoint = true,
                attacks = new (),
            };
            
            node.styleSheets.Add(Resources.Load<StyleSheet>("Stylesheets/ComboNode"));

            var addPortBtn = new Button(() => AddChoicePort(node));
            addPortBtn.text = "Add Choice";
            node.titleContainer.Add(addPortBtn); 
            
            node.capabilities &= ~Capabilities.Movable;
            node.capabilities &= ~Capabilities.Deletable;
            
            node.RefreshExpandedState();
            node.RefreshPorts();
            
            node.SetPosition(new Rect(100, 200, 100, 150));
            entryPointNode = node;
            AddElement(node);
        }

        private Port GeneratePort(ComboGraphNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); //Arbitrary Type
        }

        public void AddChoicePort(ComboGraphNode comboGraphNode, string overridePortName = "")
        {
            var choicePort = GeneratePort(comboGraphNode, Direction.Output);
            
            var oldLabel = choicePort.contentContainer.Q<Label>("type");
            choicePort.contentContainer.Remove(oldLabel);

            int outputPortCount = comboGraphNode.outputContainer.Query("connector").ToList().Count;
            var choicePortName = string.IsNullOrEmpty(overridePortName) ? $"Choice {outputPortCount}" : overridePortName;
            
            var textField = new TextField
            {
                name = string.Empty,
                value = choicePortName
            };
            
            textField.RegisterValueChangedCallback(evt => choicePort.portName = evt.newValue);
            choicePort.contentContainer.Add(new Label("O"));
            choicePort.contentContainer.Add(textField);
            var deleteButton = new Button(() => RemovePort(comboGraphNode, choicePort))
            {
                text = "X",
            };
            choicePort.contentContainer.Add(deleteButton);
            
            choicePort.portName = choicePortName;
            comboGraphNode.outputContainer.Add(choicePort);
            comboGraphNode.RefreshPorts();
            comboGraphNode.RefreshExpandedState();
        }

        private void RemovePort(ComboGraphNode node, Port port)
        {
            var targetEdge = edges.ToList()
                .Where(edge => edge.output.portName == port.portName && edge.output.node == port.node);

            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(edge);
            }
            
            node.outputContainer.Remove(port);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        public void CreateNode(string nodeName, Vector2 position)
        {
            AddElement(CreateComboNode(nodeName, position));
        }

        public ComboGraphNode CreateComboNode(string nodeName, Vector2 position, List<string> attacks = null)
        {
            var node = new ComboGraphNode
            {
                guid = SerializableGuid.NewGuid(),
                title = nodeName,
                nodeName = nodeName,
                attacks = attacks ?? new()
            };
            
            node.styleSheets.Add(Resources.Load<StyleSheet>("Stylesheets/ComboNode"));
            
            var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);
            
            var addPortBtn = new Button(() => AddChoicePort(node));
            addPortBtn.text = "Add Choice";
            node.titleContainer.Add(addPortBtn);

            var listView = new ListView
            {
                itemsSource = node.attacks,
                makeItem = () => new TextField(),
                bindItem = (element, i) =>
                {
                    var textField = (TextField)element;
                    textField.value = node.attacks[i];
                    textField.RegisterValueChangedCallback(evt => node.attacks[i] = evt.newValue);
                },
                selectionType = SelectionType.Single,
                reorderable = true,
                allowAdd = true,
                allowRemove = true
            };
            var buttonAdd = new Button(() =>
            {
                node.attacks.Add(string.Empty);
                listView.Rebuild();
            }) { text = "+" };
            var buttonRemove = new Button(() =>
            {
                if(node.attacks.Count == 0) return;
                node.attacks.RemoveAt(node.attacks.Count - 1);
                listView.Rebuild();
            }) { text = "-" };
            node.contentContainer.Add(listView);
            node.contentContainer.Add(buttonAdd);
            node.contentContainer.Add(buttonRemove);

            var textField = new TextField(string.Empty);
            textField.RegisterValueChangedCallback(evt =>
            {
                node.nodeName = evt.newValue;
                node.title = evt.newValue;
            });
            textField.SetValueWithoutNotify(node.title);
            node.mainContainer.Add(textField);
            
            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(position, defaultNodeSize));
            
            return node;
        }

        public void ResetEntry()
        {
            entryPointNode?.outputContainer.Clear();
        }
    }
}